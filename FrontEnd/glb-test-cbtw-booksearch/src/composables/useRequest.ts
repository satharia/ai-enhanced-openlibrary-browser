import { shallowRef } from 'vue'

// Loading and error state for one request at a time: running it again aborts the request still in flight
export function useRequest<T>(sendRequest: (signal: AbortSignal) => Promise<T>) {
  const data = shallowRef<T | null>(null)
  const isLoading = shallowRef(false)
  const errorMessage = shallowRef<string | null>(null)

  let requestInFlight: AbortController | null = null

  async function run() {
    requestInFlight?.abort()
    const request = new AbortController()
    requestInFlight = request

    isLoading.value = true
    errorMessage.value = null

    try {
      data.value = await sendRequest(request.signal)
    } catch (error) {
      if (request.signal.aborted) return
      errorMessage.value = error instanceof Error ? error.message : 'Request failed'
    } finally {
      if (requestInFlight === request) isLoading.value = false
    }
  }

  return { data, isLoading, errorMessage, run }
}
