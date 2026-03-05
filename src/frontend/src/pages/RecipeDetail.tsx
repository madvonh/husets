import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { getRecipe, addTagToRecipe, removeTagFromRecipe, ApiError } from '../services/api'
import type { Recipe } from '../types'

export default function RecipeDetail() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [recipe, setRecipe] = useState<Recipe | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<{ message: string; correlationId?: string } | null>(null)
  const [newTag, setNewTag] = useState('')
  const [isAddingTag, setIsAddingTag] = useState(false)
  const [removingTag, setRemovingTag] = useState<string | null>(null)

  useEffect(() => {
    if (!id) {
      navigate('/')
      return
    }

    loadRecipe(id)
  }, [id, navigate])

  const loadRecipe = async (recipeId: string) => {
    setIsLoading(true)
    setError(null)

    try {
      const data = await getRecipe(recipeId)
      setRecipe(data)
    } catch (err) {
      if (err instanceof ApiError) {
        setError({ message: err.message, correlationId: err.correlationId })
      } else {
        setError({ message: 'Failed to load recipe. Please try again.' })
      }
    } finally {
      setIsLoading(false)
    }
  }

  const handleAddTag = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id || !newTag.trim() || isAddingTag) return

    setIsAddingTag(true)
    setError(null)

    try {
      const updatedRecipe = await addTagToRecipe(id, newTag.trim())
      setRecipe(updatedRecipe)
      setNewTag('')
    } catch (err) {
      if (err instanceof ApiError) {
        setError({ message: err.message, correlationId: err.correlationId })
      } else {
        setError({ message: 'Failed to add tag. Please try again.' })
      }
    } finally {
      setIsAddingTag(false)
    }
  }

  const handleRemoveTag = async (tag: string) => {
    if (!id || removingTag) return

    setRemovingTag(tag)
    setError(null)

    try {
      const updatedRecipe = await removeTagFromRecipe(id, tag)
      setRecipe(updatedRecipe)
    } catch (err) {
      if (err instanceof ApiError) {
        setError({ message: err.message, correlationId: err.correlationId })
      } else {
        setError({ message: 'Failed to remove tag. Please try again.' })
      }
    } finally {
      setRemovingTag(null)
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading recipe...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="container mx-auto px-4 py-8 max-w-4xl">
          <button
            onClick={() => navigate('/')}
            className="text-blue-600 hover:text-blue-800 mb-6"
          >
            ← Back to Home
          </button>
          <div className="bg-red-50 border border-red-200 rounded-lg p-6">
            <h2 className="text-xl font-semibold text-red-800 mb-2">Error Loading Recipe</h2>
            <p className="text-red-700">{error.message}</p>
            {error.correlationId && (
              <p className="text-red-600 text-sm mt-2">Correlation ID: {error.correlationId}</p>
            )}
            <button
              onClick={() => loadRecipe(id!)}
              className="mt-4 bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
            >
              Retry
            </button>
          </div>
        </div>
      </div>
    )
  }

  if (!recipe) {
    return null
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8 max-w-4xl">
        <div className="mb-6">
          <button
            onClick={() => navigate('/')}
            className="text-blue-600 hover:text-blue-800 flex items-center gap-2"
          >
            ← Back to Home
          </button>
        </div>

        {/* Recipe Header */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h1 className="text-3xl md:text-4xl font-bold mb-4 text-gray-900">{recipe.title}</h1>
          
          {/* Tags Display */}
          <div className="mb-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Tags</h3>
            <div className="flex flex-wrap gap-2 mb-3">
              {recipe.tags && recipe.tags.length > 0 ? (
                recipe.tags.map((tag, index) => (
                  <span
                    key={index}
                    className="inline-flex items-center gap-2 px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm font-medium"
                  >
                    {tag}
                    <button
                      onClick={() => handleRemoveTag(tag)}
                      disabled={removingTag === tag}
                      className="hover:text-blue-900 focus:outline-none disabled:opacity-50"
                      aria-label={`Remove tag ${tag}`}
                    >
                      {removingTag === tag ? '...' : '×'}
                    </button>
                  </span>
                ))
              ) : (
                <span className="text-gray-500 text-sm">No tags yet</span>
              )}
            </div>

            {/* Add Tag Form */}
            <form onSubmit={handleAddTag} className="flex flex-col sm:flex-row gap-2">
              <input
                type="text"
                value={newTag}
                onChange={(e) => setNewTag(e.target.value)}
                placeholder="Add a tag (e.g., dessert, dinner...)"
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
                disabled={isAddingTag}
              />
              <button
                type="submit"
                disabled={!newTag.trim() || isAddingTag}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-sm font-medium transition-colors"
              >
                {isAddingTag ? 'Adding...' : 'Add Tag'}
              </button>
            </form>
          </div>

          <p className="text-sm text-gray-600">
            Added on {new Date(recipe.createdAt).toLocaleDateString()}
          </p>
        </div>

        {/* Ingredients */}
        {recipe.ingredients && recipe.ingredients.length > 0 && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <h2 className="text-2xl font-semibold mb-4 text-gray-900">Ingredients</h2>
            <ul className="space-y-2">
              {recipe.ingredients
                .sort((a, b) => a.position - b.position)
                .map((ingredient, index) => (
                  <li key={index} className="flex items-start gap-3">
                    <span className="text-blue-600 mt-1">•</span>
                    <div className="flex-1">
                      <span className="text-gray-800">{ingredient.freeText}</span>
                      {ingredient.canonicalName && (
                        <span className="text-gray-500 text-sm ml-2">
                          ({ingredient.canonicalName})
                        </span>
                      )}
                    </div>
                  </li>
                ))}
            </ul>
          </div>
        )}

        {/* Full Recipe Text */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h2 className="text-2xl font-semibold mb-4 text-gray-900">Recipe</h2>
          <div className="whitespace-pre-wrap text-gray-800 leading-relaxed">
            {recipe.rawText}
          </div>
        </div>

        {/* Image Reference */}
        {recipe.imageRef && (
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-2xl font-semibold mb-4 text-gray-900">Original Photo</h2>
            <p className="text-sm text-gray-600 mb-2">
              Image stored at: <code className="bg-gray-100 px-2 py-1 rounded">{recipe.imageRef}</code>
            </p>
            <p className="text-xs text-gray-500">
              Note: Image display requires additional configuration in production
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
