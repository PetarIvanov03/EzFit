import { useState } from "react"
import { useLogEntry } from "@/api/log"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Input } from "@/components/ui/input"
import { Separator } from "@/components/ui/separator"
import { EntryCard } from "@/components/entries/EntryCard"
import { todayLocalDateString } from "@/lib/format"

export function AddEntryPage() {
  const [date, setDate] = useState(todayLocalDateString())
  const [text, setText] = useState("")
  const [files, setFiles] = useState<File[]>([])

  const logEntry = useLogEntry()

  const canSubmit = text.trim().length > 0 || files.length > 0

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    logEntry.mutate(
      { date, message: text.trim() || undefined, images: files.length > 0 ? files : undefined },
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
          <form onSubmit={handleSubmit} className="flex flex-col gap-5">
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

            <div className="flex flex-col gap-2">
              <Label htmlFor="entry-text">Free text</Label>
              <Textarea
                id="entry-text"
                placeholder="e.g. Had chicken and rice for lunch, then ran 5k this morning..."
                rows={5}
                value={text}
                onChange={(e) => setText(e.target.value)}
              />
            </div>

            <div className="flex items-center gap-3">
              <Separator className="flex-1" />
              <span className="text-xs text-muted-foreground">OR</span>
              <Separator className="flex-1" />
            </div>

            <div className="flex flex-col gap-2">
              <Label htmlFor="entry-file">Screenshot upload</Label>
              <Input
                id="entry-file"
                type="file"
                accept="image/*"
                multiple
                onChange={(e) => setFiles(Array.from(e.target.files ?? []))}
              />
              {files.length > 0 && (
                <p className="text-sm text-muted-foreground">
                  Selected: {files.map((f) => f.name).join(", ")}
                </p>
              )}
            </div>

            <Button type="submit" disabled={!canSubmit || logEntry.isPending} className="w-fit">
              {logEntry.isPending ? "Submitting..." : "Submit for extraction"}
            </Button>

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
