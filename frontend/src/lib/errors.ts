import axios from "axios"

function readProblemDetail(data: unknown): string | null {
  if (typeof data === "string" && data.trim().length > 0) {
    // Explicit BadRequest("...") results from the controller serialize as a bare
    // JSON string, not a ProblemDetails object — handle both shapes.
    return data
  }

  if (data && typeof data === "object" && "detail" in data) {
    const detail = (data as { detail?: unknown }).detail
    if (typeof detail === "string" && detail.trim().length > 0) {
      return detail
    }
  }

  return null
}

// Axios's own error.message ("Request failed with status code 502") is never what a
// user should see — the backend's ProblemDetails "detail" (or a plain BadRequest string)
// is the actual, client-safe explanation where the server provided one.
export function getApiErrorMessage(error: unknown, fallback = "Something went wrong. Please try again."): string {
  if (axios.isAxiosError(error)) {
    const detail = readProblemDetail(error.response?.data)
    if (detail) return detail

    if (!error.response) {
      return "Could not reach the server. Check your connection and try again."
    }

    return `${fallback} (status ${error.response.status})`
  }

  if (error instanceof Error && error.message) {
    return error.message
  }

  return fallback
}
