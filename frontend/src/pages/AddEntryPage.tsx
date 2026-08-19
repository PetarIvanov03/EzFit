import { useRef, useState } from "react"
import { Paperclip, SendHorizontal } from "lucide-react"
import { useLogEntry } from "@/api/log"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Input } from "@/components/ui/input"
import { EntryCard } from "@/components/entries/EntryCard"
import { AttachmentThumbnail } from "@/components/composer/AttachmentThumbnail"
import { todayLocalDateString } from "@/lib/format"
import { downscaleImages, validateImageDimensions } from "@/lib/image"
import { useIsFinePointer } from "@/hooks/useIsFinePointer"

// Same underlying file re-picked from the OS dialog gets a fresh File
// instance each time, so identity dedup has to go by these fields instead.
function fileKey(file: File) {
  return `${file.name}:${file.size}:${file.lastModified}`
}

export function AddEntryPage() {
  const [date, setDate] = useState(todayLocalDateString())
  const [text, setText] = useState("")
  const [files, setFiles] = useState<File[]>([])
  const [isProcessingImages, setIsProcessingImages] = useState(false)
  const [imageError, setImageError] = useState<string | null>(null)

  const fileInputRef = useRef<HTMLInputElement>(null)
  const logEntry = useLogEntry()
  const isFinePointer = useIsFinePointer()

  // Images alone are not a valid submission — the backend rejects that too —
  // so send only ever unlocks once there's text, regardless of attachments.
  const hasText = text.trim().length > 0
  const needsDescriptionHint = !hasText && files.length > 0
  const canSubmit = hasText && !isProcessingImages

  function handleFilesSelected(e: React.ChangeEvent<HTMLInputElement>) {
    const selected = Array.from(e.target.files ?? [])
    if (selected.length > 0) {
      setFiles((prev) => {
        const seen = new Set(prev.map(fileKey))
        const additions = selected.filter((file) => {
          const key = fileKey(file)
          if (seen.has(key)) return false
          seen.add(key)
          return true
        })
        return [...prev, ...additions]
      })
    }
    // Reset so picking the exact same file(s) again still fires this handler.
    e.target.value = ""
  }

  function handleRemoveFile(file: File) {
    setFiles((prev) => prev.filter((f) => f !== file))
  }

  function handleTextKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    // Touch devices have no Shift+Enter, so Enter must stay a plain newline
    // there — the send button is the only submit path on those devices.
    if (!isFinePointer) return

    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault()
      e.currentTarget.form?.requestSubmit()
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setImageError(null)

    let imagesToSend: File[] | undefined

    if (files.length > 0) {
      setIsProcessingImages(true)
      try {
        for (const file of files) {
          const result = await validateImageDimensions(file)
          if (!result.valid) {
            setImageError(result.message ?? "One of the selected images is too large.")
            return
          }
        }
        imagesToSend = await downscaleImages(files)
      } finally {
        setIsProcessingImages(false)
      }
    }

    logEntry.mutate(
      { date, message: text.trim() || undefined, images: imagesToSend },
      {
        onSuccess: () => {
          setText("")
          setFiles([])
        },
      },
    )
  }

  return (
    <div className="flex flex-col gap-4">
      <h1 className="text-2xl font-semibold">Add Entry</h1>

      <Card>
        <CardHeader>
          <CardTitle className="text-base font-medium">
            Describe it, or upload a screenshot
          </CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="flex flex-col gap-3">
            <div className="flex flex-col gap-2">
              <Label htmlFor="entry-date">Date</Label>
              <Input
                id="entry-date"
                type="date"
                value={date}
                onChange={(e) => setDate(e.target.value)}
                className="w-fit"
              />
            </div>

            {files.length > 0 && (
              // gap-3 (not gap-2) so each thumbnail's enlarged remove hit-area
              // (see AttachmentThumbnail) has room on the right without
              // reaching the next tile — see the comment there for the math.
              <div className="flex gap-3 overflow-x-auto pb-1">
                {files.map((file) => (
                  <AttachmentThumbnail
                    key={fileKey(file)}
                    file={file}
                    onRemove={() => handleRemoveFile(file)}
                  />
                ))}
              </div>
            )}

            <div className="flex items-end gap-2 rounded-md border border-input bg-transparent p-2 focus-within:border-ring focus-within:ring-[3px] focus-within:ring-ring/50">
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                multiple
                onChange={handleFilesSelected}
                className="hidden"
                aria-label="Attach screenshots"
              />
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="shrink-0"
                aria-label="Attach screenshots"
                onClick={() => fileInputRef.current?.click()}
              >
                <Paperclip className="size-6" />
              </Button>

              <Textarea
                id="entry-text"
                aria-label="Entry description"
                placeholder="Describe it, or attach a screenshot..."
                rows={1}
                value={text}
                onChange={(e) => setText(e.target.value)}
                onKeyDown={handleTextKeyDown}
                className="max-h-40 min-h-9 flex-1 resize-none overflow-y-auto border-0 bg-transparent px-1 py-1.5 text-base shadow-none focus-visible:ring-0"
              />

              <Button
                type="submit"
                size="icon"
                className="shrink-0"
                aria-label="Send"
                disabled={!canSubmit || logEntry.isPending}
              >
                <SendHorizontal className="size-6" />
              </Button>
            </div>

            {needsDescriptionHint && (
              <p className="text-sm text-muted-foreground">
                Add a short description alongside your screenshots — images alone aren't enough.
              </p>
            )}

            {isProcessingImages && (
              <p className="text-sm text-muted-foreground">Processing images...</p>
            )}

            {imageError && <p className="text-sm text-destructive">{imageError}</p>}

            {logEntry.isError && (
              <p className="text-sm text-destructive">
                {logEntry.error instanceof Error
                  ? logEntry.error.message
                  : "Submission failed. Please try again."}
              </p>
            )}
          </form>
        </CardContent>
      </Card>

      {logEntry.isSuccess && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base font-medium">Result</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            {logEntry.data.createdEntries.length > 0 && (
              <div className="flex flex-col gap-2">
                <p className="text-sm font-medium text-muted-foreground">
                  Saved ({logEntry.data.createdEntries.length})
                </p>
                <div className="flex flex-col gap-2">
                  {logEntry.data.createdEntries.map((entry) => (
                    <EntryCard key={entry.id} entry={entry} />
                  ))}
                </div>
              </div>
            )}

            {logEntry.data.rejectionReasons.length > 0 && (
              <div className="flex flex-col gap-2">
                <p className="text-sm font-medium text-muted-foreground">
                  Couldn't parse
                </p>
                <ul className="list-inside list-disc text-sm text-destructive">
                  {logEntry.data.rejectionReasons.map((reason, i) => (
                    <li key={i}>{reason}</li>
                  ))}
                </ul>
              </div>
            )}

            {logEntry.data.createdEntries.length === 0 &&
              logEntry.data.rejectionReasons.length === 0 && (
                <p className="text-sm text-muted-foreground">
                  Nothing came back from this submission.
                </p>
              )}

            <Button variant="outline" className="w-fit" onClick={() => logEntry.reset()}>
              Log another entry
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
