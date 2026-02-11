import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { searchRecipes, ApiError } from '../services/api'
import type { RecipeSummary } from '../types'

export default function Home() {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [tag, setTag] = useState('')
  const [recipes, setRecipes] = useState<RecipeSummary[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadRecipes()
  }, [])

  const loadRecipes = async (searchQuery?: string, searchTag?: string) => {
    setIsLoading(true)
    setError(null)

    try {
      const results = await searchRecipes(searchQuery, searchTag)
      setRecipes(results)
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
      } else {
        setError('Failed to load recipes. Please try again.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    loadRecipes(query, tag)
  }

  const handleClearSearch = () => {
    setQuery('')
    setTag('')
    loadRecipes()
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    })
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8 max-w-6xl">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-4xl md:text-5xl font-bold mb-4 text-gray-900">Recipe Collection</h1>
          <p className="text-lg text-gray-600">
            Search your digitized recipes or add new ones
          </p>
        </div>

        {/* Add Recipe Button */}
        <div className="mb-8 text-center">
          <button
            onClick={() => navigate('/add-recipe')}
            className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 font-medium transition-colors inline-flex items-center gap-2"
          >
            <span className="text-xl">+</span>
            Add Recipe
          </button>
        </div>

        {/* Search Form */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <form onSubmit={handleSearch} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label htmlFor="query" className="block text-sm font-medium text-gray-700 mb-2">
                  Search by keyword
                </label>
                <input
                  id="query"
                  type="text"
                  value={query}
                  onChange={(e) => setQuery(e.target.value)}
                  placeholder="e.g., chocolate, pasta, chicken..."
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
              <div>
                <label htmlFor="tag" className="block text-sm font-medium text-gray-700 mb-2">
                  Filter by tag
                </label>
                <input
                  id="tag"
                  type="text"
                  value={tag}
                  onChange={(e) => setTag(e.target.value)}
                  placeholder="e.g., dessert, dinner, vegetarian..."
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
            </div>
            <div className="flex flex-col sm:flex-row gap-3">
              <button
                type="submit"
                className="flex-1 sm:flex-none bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 font-medium transition-colors"
              >
                Search
              </button>
              {(query || tag) && (
                <button
                  type="button"
                  onClick={handleClearSearch}
                  className="flex-1 sm:flex-none bg-gray-200 text-gray-700 px-6 py-2 rounded-lg hover:bg-gray-300 font-medium transition-colors"
                >
                  Clear
                </button>
              )}
            </div>
          </form>
        </div>

        {/* Error State */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-8">
            <p className="text-red-700">{error}</p>
            <button
              onClick={() => loadRecipes()}
              className="mt-2 text-red-600 hover:text-red-800 underline"
            >
              Retry
            </button>
          </div>
        )}

        {/* Loading State */}
        {isLoading && (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">Loading recipes...</p>
          </div>
        )}

        {/* Results */}
        {!isLoading && !error && (
          <div>
            <div className="mb-4 text-gray-600">
              {recipes.length === 0 ? (
                <p>No recipes found. {query || tag ? 'Try a different search.' : 'Add your first recipe to get started!'}</p>
              ) : (
                <p>{recipes.length} recipe{recipes.length !== 1 ? 's' : ''} found</p>
              )}
            </div>

            {recipes.length > 0 && (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {recipes.map((recipe) => (
                  <div
                    key={recipe.id}
                    onClick={() => navigate(`/recipe/${recipe.id}`)}
                    className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow cursor-pointer overflow-hidden"
                  >
                    <div className="p-6">
                      <h3 className="text-xl font-semibold mb-2 text-gray-900 line-clamp-2">
                        {recipe.title}
                      </h3>
                      <p className="text-sm text-gray-500 mb-3">
                        {formatDate(recipe.createdAt)}
                      </p>
                      {recipe.tags.length > 0 && (
                        <div className="flex flex-wrap gap-2">
                          {recipe.tags.slice(0, 3).map((t) => (
                            <span
                              key={t}
                              className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                            >
                              {t}
                            </span>
                          ))}
                          {recipe.tags.length > 3 && (
                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-600">
                              +{recipe.tags.length - 3} more
                            </span>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
