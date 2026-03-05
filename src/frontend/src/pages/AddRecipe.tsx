import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { uploadImageForOcr, createRecipe, ApiError } from '../services/api'

export default function AddRecipe() {
  const navigate = useNavigate()
  const [imageFile, setImageFile] = useState<File | null>(null)
  const [imagePreview, setImagePreview] = useState<string | null>(null)
  const [ocrText, setOcrText] = useState('')
  const [title, setTitle] = useState('')
  const [tagsInput, setTagsInput] = useState('')
  const [imageRef, setImageRef] = useState('')
  const [isProcessing, setIsProcessing] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<{ message: string; correlationId?: string } | null>(null)
  const [ocrComplete, setOcrComplete] = useState(false)

  const handleImageSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      setImageFile(file)
      setImagePreview(URL.createObjectURL(file))
      setOcrComplete(false)
      setOcrText('')
      setImageRef('')
      setError(null)
    }
  }

  const handleExtractText = async () => {
    if (!imageFile) return

    setIsProcessing(true)
    setError(null)

    try {
      const result = await uploadImageForOcr(imageFile)
      setOcrText(result.extractedText)
      setImageRef(result.imageRef)
      setOcrComplete(true)

      // Auto-extract title from first line if available
      const firstLine = result.extractedText.split('\n')[0]?.trim()
      if (firstLine && firstLine.length < 100) {
        setTitle(firstLine)
      }
    } catch (err) {
      if (err instanceof ApiError) {
        setError({ message: err.message, correlationId: err.correlationId })
      } else {
        setError({ message: 'Failed to process image. Please try again.' })
      }
    } finally {
      setIsProcessing(false)
    }
  }

  const handleSaveRecipe = async () => {
    if (!title.trim() || !ocrText.trim() || !imageRef) {
      setError({ message: 'Please provide a title and extract text from an image first.' })
      return
    }

    setIsSaving(true)
    setError(null)

    try {
      const tags = tagsInput
        .split(',')
        .map(t => t.trim())
        .filter(t => t.length > 0)

      const recipe = await createRecipe({
        title: title.trim(),
        rawText: ocrText,
        imageRef,
        tags: tags.length > 0 ? tags : undefined,
      })

      // Navigate to the new recipe
      navigate(`/recipes/${recipe.id}`)
    } catch (err) {
      if (err instanceof ApiError) {
        setError({ message: err.message, correlationId: err.correlationId })
      } else {
        setError({ message: 'Failed to save recipe. Please try again.' })
      }
    } finally {
      setIsSaving(false)
    }
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

        <h1 className="text-3xl md:text-4xl font-bold mb-8 text-gray-900">Add Recipe from Photo</h1>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800 font-medium">{error.message}</p>
            {error.correlationId && (
              <p className="text-red-600 text-sm mt-1">Correlation ID: {error.correlationId}</p>
            )}
          </div>
        )}

        {/* Step 1: Upload Image */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h2 className="text-xl font-semibold mb-4 text-gray-900">Step 1: Upload Recipe Photo</h2>
          
          <div className="space-y-4">
            <div>
              <label
                htmlFor="image-upload"
                className="block w-full px-4 py-8 border-2 border-dashed border-gray-300 rounded-lg text-center cursor-pointer hover:border-blue-500 hover:bg-blue-50 transition-colors"
              >
                {imagePreview ? (
                  <div>
                    <img
                      src={imagePreview}
                      alt="Preview"
                      className="max-h-64 mx-auto mb-2 rounded"
                    />
                    <p className="text-sm text-gray-600">Click to change image</p>
                  </div>
                ) : (
                  <div>
                    <svg
                      className="mx-auto h-12 w-12 text-gray-400"
                      stroke="currentColor"
                      fill="none"
                      viewBox="0 0 48 48"
                      aria-hidden="true"
                    >
                      <path
                        d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02"
                        strokeWidth={2}
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                    <p className="mt-2 text-sm text-gray-600">
                      Click to upload or drag and drop
                    </p>
                    <p className="text-xs text-gray-500">PNG, JPG up to 10MB</p>
                  </div>
                )}
              </label>
              <input
                id="image-upload"
                type="file"
                accept="image/jpeg,image/png"
                onChange={handleImageSelect}
                className="hidden"
              />
            </div>

            {imageFile && !ocrComplete && (
              <button
                onClick={handleExtractText}
                disabled={isProcessing}
                className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed font-medium transition-colors"
              >
                {isProcessing ? 'Extracting Text...' : 'Extract Text from Image'}
              </button>
            )}
          </div>
        </div>

        {/* Step 2: Review & Edit Text */}
        {ocrComplete && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <h2 className="text-xl font-semibold mb-4 text-gray-900">Step 2: Review & Edit</h2>

            <div className="space-y-4">
              <div>
                <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-2">
                  Recipe Title *
                </label>
                <input
                  id="title"
                  type="text"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="e.g., Chocolate Chip Cookies"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <div>
                <label htmlFor="text" className="block text-sm font-medium text-gray-700 mb-2">
                  Recipe Text *
                </label>
                <textarea
                  id="text"
                  value={ocrText}
                  onChange={(e) => setOcrText(e.target.value)}
                  rows={12}
                  placeholder="Edit the extracted text..."
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono text-sm"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Edit the text above to fix any OCR errors
                </p>
              </div>

              <div>
                <label htmlFor="tags" className="block text-sm font-medium text-gray-700 mb-2">
                  Tags (optional)
                </label>
                <input
                  id="tags"
                  type="text"
                  value={tagsInput}
                  onChange={(e) => setTagsInput(e.target.value)}
                  placeholder="dessert, cookies, baking (comma-separated)"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <button
                onClick={handleSaveRecipe}
                disabled={isSaving || !title.trim()}
                className="w-full bg-green-600 text-white py-3 px-4 rounded-lg hover:bg-green-700 disabled:bg-gray-400 disabled:cursor-not-allowed font-medium transition-colors"
              >
                {isSaving ? 'Saving Recipe...' : 'Save Recipe'}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
