export interface Recipe {
  id: string
  title: string
  rawText: string
  imageRef: string
  tags: string[]
  ingredients: Ingredient[]
  createdAt: string
}

export interface Ingredient {
  freeText: string
  canonicalName?: string
  position: number
}

export interface OcrResponse {
  imageRef: string
  extractedText: string
}

export interface ErrorResponse {
  code: string
  message: string
  correlationId?: string
}

export interface CreateRecipeRequest {
  title: string
  rawText: string
  imageRef: string
  tags?: string[]
}

export interface RecipeSummary {
  id: string
  title: string
  tags: string[]
  createdAt: string
  imageRef: string
}
