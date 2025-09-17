# Environment Configuration for Chat Interface

## Development Environment
- Frontend runs on: http://localhost:5174 (or next available port)
- API runs on: http://localhost:5107

## Production Environment
- Both frontend and API run on the same domain
- API service automatically detects the environment

## Configuration Notes

The API service (`src/services/apiService.ts`) automatically handles environment detection:
- **Development**: Uses `http://localhost:5107` for API calls
- **Production**: Uses the same domain as the frontend

If you need to change the API URL, modify the `baseUrl` in `apiService.ts`:

```typescript
this.baseUrl = import.meta.env.DEV 
  ? 'http://localhost:5107'  // Change this for development
  : window.location.origin;
```

## CORS Configuration

Make sure your .NET API has CORS configured to allow requests from the frontend:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```
