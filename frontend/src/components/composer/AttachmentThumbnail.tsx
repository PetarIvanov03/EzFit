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
        className="absolute -top-1.5 -right-1.5 flex size-6 items-center justify-center rounded-full border border-input bg-background text-foreground shadow-xs hover:bg-accent"
      >
        <X className="size-3.5" />
      </button>
    </div>
  )
}
