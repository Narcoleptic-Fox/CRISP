import { Button } from "@/components/ui/button"

function App() {
  return (
    <div className="min-h-screen bg-white">
      <header className="border-b border-gray-200">
        <div className="container mx-auto flex h-16 items-center justify-between px-4">
          <h1 className="text-2xl font-bold">CRISP</h1>
          <nav className="flex items-center gap-4">
            <Button variant="ghost">Documentation</Button>
            <Button variant="outline">Sign In</Button>
          </nav>
        </div>
      </header>
      
      <main className="container mx-auto px-4 py-16">
        <div className="mx-auto max-w-3xl text-center">
          <h2 className="text-4xl font-bold tracking-tight text-gray-900 sm:text-6xl">
            Command Response Interface Service Pattern
          </h2>
          <p className="mt-6 text-lg text-gray-600">
            A modern CQRS architectural pattern for .NET applications. 
            Direct service contracts, no mediator overhead, full type safety.
          </p>
          <div className="mt-10 flex items-center justify-center gap-4">
            <Button size="lg">Get Started</Button>
            <Button variant="outline" size="lg">
              View on GitHub
            </Button>
          </div>
        </div>

        <div className="mt-24 grid gap-8 sm:grid-cols-3">
          <div className="rounded-lg border border-gray-200 p-6">
            <h3 className="text-lg font-semibold">🚀 Better Performance</h3>
            <p className="mt-2 text-gray-600">
              No reflection overhead from mediator patterns.
            </p>
          </div>
          <div className="rounded-lg border border-gray-200 p-6">
            <h3 className="text-lg font-semibold">🔍 Better IDE Support</h3>
            <p className="mt-2 text-gray-600">
              Full IntelliSense and navigation.
            </p>
          </div>
          <div className="rounded-lg border border-gray-200 p-6">
            <h3 className="text-lg font-semibold">🧪 Simpler Testing</h3>
            <p className="mt-2 text-gray-600">
              Direct service mocking without complex setup.
            </p>
          </div>
        </div>
      </main>
    </div>
  )
}

export default App
