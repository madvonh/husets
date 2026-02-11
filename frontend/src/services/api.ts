import type { OcrResponse, Recipe, RecipeSummary, CreateRecipeRequest, ErrorResponse } from '../types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001'

class ApiError extends Error {
  constructor(
    message: string,
    public code: string,
    public correlationId?: string
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  const correlationId = response.headers.get('X-Correlation-Id') || undefined

  if (!response.ok) {
    let errorData: ErrorResponse | null = null
    try {
      errorData = await response.json()
    } catch {
      // If JSON parsing fails, use status text
    }

    throw new ApiError(
      errorData?.message || response.statusText || 'Request failed',
      errorData?.code || `HTTP_${response.status}`,
      correlationId || errorData?.correlationId
    )
  }

  return response.json()
}

export async function uploadImageForOcr(imageFile: File): Promise<OcrResponse> {
  const formData = new FormData()
  formData.append('image', imageFile)

  const response = await fetch(`${API_BASE_URL}/ocr`, {
    method: 'POST',
    body: formData,
  })

  return handleResponse<OcrResponse>(response)
}

export async function createRecipe(request: CreateRecipeRequest): Promise<Recipe> {
  const response = await fetch(`${API_BASE_URL}/recipes`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  return handleResponse<Recipe>(response)
}

export async function getRecipe(id: string): Promise<Recipe> {
  const response = await fetch(`${API_BASE_URL}/recipes/${id}`)
  return handleResponse<Recipe>(response)
}

export async function searchRecipes(query?: string, tag?: string): Promise<RecipeSummary[]> {
  const params = new URLSearchParams()
  if (query) params.append('query', query)
  if (tag) params.append('tag', tag)
  
  const url = params.toString() ? `${API_BASE_URL}/recipes?${params.toString()}` : `${API_BASE_URL}/recipes`
  const response = await fetch(url)
  return handleResponse<RecipeSummary[]>(response)
}

export async function addTagToRecipe(recipeId: string, tag: string): Promise<Recipe> {
  const response = await fetch(`${API_BASE_URL}/recipes/${recipeId}/tags`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ tag }),
  })
  return handleResponse<Recipe>(response)
}

export async function removeTagFromRecipe(recipeId: string, tag: string): Promise<Recipe> {
  const response = await fetch(`${API_BASE_URL}/recipes/${recipeId}/tags/${encodeURIComponent(tag)}`, {
    method: 'DELETE',
  })
  return handleResponse<Recipe>(response)
}

export { ApiError }
