import { Routes, Route } from 'react-router-dom'
import Home from './pages/Home'
import AddRecipe from './pages/AddRecipe'
import RecipeDetail from './pages/RecipeDetail'

function App() {
  return (
    <div className="min-h-screen bg-gray-50">
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/add-recipe" element={<AddRecipe />} />
        <Route path="/recipes/:id" element={<RecipeDetail />} />
      </Routes>
    </div>
  )
}

export default App
