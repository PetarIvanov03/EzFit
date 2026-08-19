// Mirrors backend/EzFit/EzFit/appsettings.json "Uploads" section — keep in sync if
// that config changes. This is a UX shortcut only; the backend is the real
// enforcement point and re-validates every upload regardless. Per-side width/height
// caps were removed on both sides — MaxPixels alone already bounds decoded memory
// use regardless of aspect ratio, and a per-side cap only ever added false rejections
// (an ordinary 4032x3024 phone photo missed the old 4000px width cap by 32px).
export const MAX_PIXELS = 50_000_000

const TARGET_WIDTH = 1000

// Generous but bounded: a hung decode (corrupt file, browser bug) should surface as
// an error the user can act on, not an indefinite "Processing images..." spinner.
const DECODE_TIMEOUT_MS = 20_000

export interface ImageDimensions {
  width: number
  height: number
}

export interface ImageValidationResult {
  valid: boolean
  message?: string
}

function withTimeout<T>(promise: Promise<T>, ms: number, message: string): Promise<T> {
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error(message)), ms)
    promise.then(
      (value) => {
        clearTimeout(timer)
        resolve(value)
      },
      (err: unknown) => {
        clearTimeout(timer)
        reject(err)
      },
    )
  })
}

function getImageDimensions(file: File): Promise<ImageDimensions> {
  return new Promise((resolve, reject) => {
    const url = URL.createObjectURL(file)
    const img = new Image()
    img.onload = () => {
      URL.revokeObjectURL(url)
      resolve({ width: img.naturalWidth, height: img.naturalHeight })
    }
    img.onerror = () => {
      URL.revokeObjectURL(url)
      reject(new Error(`Failed to read image dimensions for "${file.name}"`))
    }
    img.src = url
  })
}

function isHeicFile(file: File): boolean {
  const type = file.type.toLowerCase()
  return type === "image/heic" || type === "image/heif" || /\.(heic|heif)$/i.test(file.name)
}

// Safari (iOS/macOS) can decode HEIC via createImageBitmap because it goes through the
// OS's own image codecs; Chrome/Firefox generally cannot. Probing is the only reliable
// way to know — there's no capability flag for "this browser can decode this codec".
async function canDecodeHeic(file: File): Promise<boolean> {
  if (typeof createImageBitmap !== "function") return false
  try {
    const bitmap = await createImageBitmap(file)
    bitmap.close()
    return true
  } catch {
    return false
  }
}

// Cheap client-side check so an oversized or unreadable screenshot is rejected
// instantly instead of after a full upload round-trip to get the backend's 400.
export async function validateImageDimensions(file: File): Promise<ImageValidationResult> {
  if (isHeicFile(file) && !(await canDecodeHeic(file))) {
    return {
      valid: false,
      message: `"${file.name}" is a HEIC/HEIF photo, which this browser can't read. Please share it as a JPEG instead (on iPhone: Settings > Camera > Formats > Most Compatible, or choose JPEG when sharing/exporting).`,
    }
  }

  let dims: ImageDimensions
  try {
    dims = await getImageDimensions(file)
  } catch (err) {
    // Can't read it client-side — let the backend be the judge rather than
    // blocking a possibly-valid upload on a local decode quirk.
    console.warn(`Skipping dimension pre-check for "${file.name}": could not read dimensions.`, err)
    return { valid: true }
  }

  const megapixels = (dims.width * dims.height) / 1_000_000
  const maxMegapixels = MAX_PIXELS / 1_000_000
  if (megapixels > maxMegapixels) {
    return {
      valid: false,
      message: `"${file.name}" is ${megapixels.toFixed(1)}MP, which is over the ${maxMegapixels}MP limit.`,
    }
  }

  return { valid: true }
}

// convertToBlob({ type: "image/webp" }) does NOT throw when the browser can't encode
// WebP — per spec it silently returns image/png instead (Safari <16.4 has no
// OffscreenCanvas at all; some versions after that still can't encode WebP). Checking
// blob.type after the fact is the only way to detect that.
//
// Fallback order is WebP -> PNG -> JPEG, not straight to JPEG: these uploads are
// text-heavy screenshots the AI has to read numbers off of, and JPEG's block artifacts
// around small text measurably hurt that. PNG is lossless and compresses flat-colour,
// sharp-edged UI screenshots well, so the size penalty over JPEG is small for this
// content specifically — worth it for the accuracy. JPEG (0.92, still high) is the
// last resort only if even PNG encoding isn't available.
async function encodeCanvas(canvas: OffscreenCanvas, fileNameBase: string): Promise<File | null> {
  const webpBlob = await canvas.convertToBlob({ type: "image/webp" })
  if (webpBlob.type === "image/webp") {
    console.info(`Encoded "${fileNameBase}" as image/webp.`)
    return new File([webpBlob], `${fileNameBase}.webp`, { type: "image/webp" })
  }

  console.warn(
    `Browser silently downgraded "${fileNameBase}" from image/webp to ${webpBlob.type || "an unknown type"}; retrying as image/png.`,
  )

  const pngBlob = await canvas.convertToBlob({ type: "image/png" })
  if (pngBlob.type === "image/png") {
    console.info(`Encoded "${fileNameBase}" as image/png (WebP unsupported on this browser).`)
    return new File([pngBlob], `${fileNameBase}.png`, { type: "image/png" })
  }

  console.warn(
    `Browser could not encode "${fileNameBase}" as image/png either (got ${pngBlob.type || "an unknown type"}); retrying as image/jpeg.`,
  )

  const jpegBlob = await canvas.convertToBlob({ type: "image/jpeg", quality: 0.92 })
  if (jpegBlob.type === "image/jpeg") {
    console.info(`Encoded "${fileNameBase}" as image/jpeg (WebP and PNG unsupported on this browser).`)
    return new File([jpegBlob], `${fileNameBase}.jpg`, { type: "image/jpeg" })
  }

  console.warn(
    `Browser could not encode "${fileNameBase}" as image/webp, image/png, or image/jpeg (got ${jpegBlob.type || "an unknown type"}); keeping the original file.`,
  )
  return null
}

// Deliberately does NOT go through a regular <canvas> sized to the image's natural
// dimensions: mobile Safari caps canvas dimensions (historically 4096-8192px per side)
// and silently produces a blank result for images this tall. createImageBitmap's resize
// options scale during decode instead, so the canvas we do touch is already at the
// small target size.
async function decodeAndEncode(file: File, baseName: string): Promise<File | null> {
  const bitmap = await createImageBitmap(file, {
    resizeWidth: TARGET_WIDTH,
    resizeQuality: "high",
  })

  const canvas = new OffscreenCanvas(bitmap.width, bitmap.height)
  const ctx = canvas.getContext("2d")
  if (!ctx) {
    bitmap.close()
    throw new Error("2D context unavailable on OffscreenCanvas")
  }

  ctx.drawImage(bitmap, 0, 0)
  bitmap.close()

  return encodeCanvas(canvas, baseName)
}

// Downscales to ~1000px wide client-side so upload size and backend decode cost both
// drop roughly tenfold for the tall screenshots this app expects.
export async function downscaleImage(file: File): Promise<File> {
  if (typeof createImageBitmap !== "function" || typeof OffscreenCanvas !== "function") {
    console.warn(
      `Skipping client-side downscale for "${file.name}": createImageBitmap/OffscreenCanvas unavailable in this browser.`,
    )
    return file
  }

  try {
    const dims = await getImageDimensions(file)
    if (dims.width <= TARGET_WIDTH) {
      return file
    }

    const baseName = file.name.replace(/\.[^.]+$/, "")
    const encoded = await withTimeout(
      decodeAndEncode(file, baseName),
      DECODE_TIMEOUT_MS,
      `Timed out decoding "${file.name}" client-side`,
    )

    if (!encoded || encoded.size === 0) {
      throw new Error("Canvas encoding produced no usable output in any supported format")
    }

    return encoded
  } catch (err) {
    console.warn(`Falling back to original file for "${file.name}": client-side downscale failed.`, err)
    return file
  }
}

export async function downscaleImages(files: File[]): Promise<File[]> {
  return Promise.all(files.map((file) => downscaleImage(file)))
}
