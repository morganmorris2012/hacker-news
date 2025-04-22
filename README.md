# Hacker News Reader

A full-stack application that displays stories from Hacker News, built with Angular 17 and .NET 7.

## Features

- **Story Browsing**
  - View newest Hacker News stories with detailed information
  - View story details including title, author, score, and URL
  - Pagination support for browsing through stories

- **Search Functionality**
  - Search stories by title or keywords
  - Paginated search results

- **Performance & Optimization**
  - Client-side caching for faster loading
  - Server-side caching to reduce API calls
  - Optimized bundle size for quick initial load

## Prerequisites

- .NET 7.0 SDK or later
- Node.js 18.x or later
- npm 9.x or later
- Angular CLI 17.x
- Git

## Project Structure

```
src/
├── hackernews-client/          # Angular Frontend
├── HackerNewsReader.Api/       # .NET API Layer
├── HackerNewsReader.Core/      # Domain Models & Interfaces
├── HackerNewsReader.Infrastructure/ # Implementation & Services
└── HackerNewsReader.Tests/     # Test Projects
```

## Getting Started

### Backend Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/hackernews-reader.git
   cd hackernews-reader
   ```

2. Navigate to the solution directory:
   ```bash
   cd src
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Run the API:
   ```bash
   cd HackerNewsReader.Api
   dotnet run
   ```

The API will be available at:
- `https://localhost:5001` (HTTPS)
- `http://localhost:5000` (HTTP)

### Frontend Setup

1. Navigate to the Angular project directory:
   ```bash
   cd src/hackernews-client
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run the development server:
   ```bash
   ng serve
   ```

The application will be available at:
- `http://localhost:4200`

## API Endpoints

- `GET /api/stories/newest` - Get newest stories with pagination
  - Query parameters: `page`, `pageSize`
- `GET /api/stories/search` - Search stories by title
  - Query parameters: `query`, `page`, `pageSize`
- `GET /api/stories/{id}` - Get a specific story by ID

## Technologies Used

- ASP.NET Core 7.0
- Angular 17
- xUnit
- Jasmine
- TypeScript
- SCSS

## Testing

### Backend Tests

```bash
cd src/HackerNewsReader.Tests
dotnet test
```

### Frontend Tests

```bash
cd src/hackernews-client
ng test
```

## Design Decisions

- **Clean Architecture**: The backend follows clean architecture principles with separate layers for API, Core, and Infrastructure.
- **Dependency Injection**: Both frontend and backend utilize dependency injection for better testability and maintainability.
- **Caching**: The backend implements caching to reduce API calls to Hacker News.
- **Error Handling**: Both frontend and backend include proper error handling and user feedback.

## Future Improvements

- Implement user authentication
- Add story comments functionality
- Add story bookmarking feature
- Implement real-time updates
- Add more comprehensive error handling
- Improve test coverage
- Add E2E tests
- Support for multiple languages
- Progressive Web App (PWA) support 