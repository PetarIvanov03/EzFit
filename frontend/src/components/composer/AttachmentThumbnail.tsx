import { useEffect, useState } from "react"
import { X } from "lucide-react"

interface AttachmentThumbnailProps {
  file: File
  onRemove: () => void
}

// Screenshots here are extremely tall (e.g. 1170x12766); a naive thumbnail of
// the whole frame becomes a useless sliver, so the tile crops to a 3:4 portrait
// and anchors to the top, where the summary numbers usually are.
export function AttachmentThumbnail({ file, onRemove }: AttachmentThumbnailProps) {
  const [url, setUrl] = useState<string | null>(null)

  useEffect(() => {
    const objectUrl = URL.createObjectURL(file)
    setUrl(objectUrl)
    return () => URL.revokeObjectURL(objectUrl)
  }, [file])

  return (
    <div className="relative w-20 shrink-0">
      <div className="aspect-[3/4] w-full overflow-hidden rounded-md border border-input bg-muted">
        {url && (
          <img src={url} alt={file.name} className="size-full object-cover object-top" />
        )}
      </div>
      <button
        type="button"
        onClick={onRemove}
        aria-label={`Remove ${file.name}`}
        // Visible badge stays a small 24px circle so it doesn't dominate an
        // 80px tile, but the actual hit target is grown via ::before to ~40px
        // (WCAG 2.5.5 floor is 24px, iOS HIG recommends 44px — full 44px would
        // spill past the 12px gap into the next thumbnail's tile, so this
        // trades down to 40px: 8px/10px on the top/left where the tile's own
        // image absorbs the overlap harmlessly, only 6px to the right where
        // the neighbouring thumbnail's hit area has to be respected).
        className="absolute -top-1 -right-1 flex size-6 items-center justify-center rounded-full border border-input bg-background text-foreground shadow-xs before:absolute before:content-[''] before:[inset:-8px_-6px_-8px_-10px] hover:bg-accent"
      >
        <X className="size-3.5" />
      </button>
    </div>
  )
}
