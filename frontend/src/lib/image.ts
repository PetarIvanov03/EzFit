// Mirrors backend/EzFit/EzFit/appsettings.json "Uploads" section — keep these in
// sync if that config changes. This is a UX shortcut only; the backend is the
// real enforcement point and re-validates every upload regardless.
export const MAX_PIXELS = 25_000_000
export const MAX_WIDTH = 4000
export const MAX_HEIGHT = 25_000

const TARGET_WIDTH = 1000

export interface ImageDimensions {
  width: number
  height: number
}

export interface ImageValidationResult {
  valid: boolean
  message?: string
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

// Cheap client-side check so an oversized screenshot is rejected instantly
// instead of after a full upload round-trip to get the backend's 400.
export async function validateImageDimensions(file: File): Promise<ImageValidationResult> {
  let dims: ImageDimensions
  try {
    dims = await getImageDimensions(file)
  } catch (err) {
    // Can't read it client-side — let the backend be the judge rather than
    // blocking a possibly-valid upload on a local decode quirk.
    console.warn(`Skipping dimension pre-check for "${file.name}": could not read dimensions.`, err)
    return { valid: true }
  }

  if (dims.width > MAX_WIDTH) {
    return {
      valid: false,
      message: `"${file.name}" is ${dims.width}px wide, which is over the ${MAX_WIDTH}px limit.`,
    }
  }

  if (dims.height > MAX_HEIGHT) {
    return {
      valid: false,
      message: `"${file.name}" is ${dims.height}px tall, which is over the ${MAX_HEIGHT}px limit.`,
    }
  }

  if (dims.width * dims.height > MAX_PIXELS) {
    return {
      valid: false,
      message: `"${file.name}" is ${dims.width}×${dims.height} (${(dims.width * dims.height / 1_000_000).toFixed(1)}MP), which is over the ${MAX_PIXELS / 1_000_000}MP limit.`,
    }
  }

  return { valid: true }
}

// Downscales to ~1000px wide WebP client-side so upload size and backend decode
// cost both drop roughly tenfold for the tall screenshots this app expects.
//
// Deliberately does NOT go through a regular <canvas> sized to the image's
// natural dimensions: mobile Safari caps canvas dimensions (historically
// 4096-8192px per side) and silently produces a blank result for images this
// tall. createImageBitmap's resize options scale during decode instead, so
// the canvas we do touch is already at the small target size.
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

    const blob = await canvas.convertToBlob({ type: "image/webp" })
    if (!blob || blob.size === 0) {
      throw new Error("convertToBlob produced an empty result")
    }

    const newName = file.name.replace(/\.[^.]+$/, "") + ".webp"
    return new File([blob], newName, { type: "image/webp" })
  } catch (err) {
    console.warn(`Falling back to original file for "${file.name}": client-side downscale failed.`, err)
    return file
  }
}

export async function downscaleImages(files: File[]): Promise<File[]> {
  return Promise.all(files.map((file) => downscaleImage(file)))
}
