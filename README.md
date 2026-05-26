# AI Visualizer App

The AI Visualizer App is a .NET Blazor-based fashion visualization system that allows customers or staff to preview custom gowns using selected styles, fabrics, colors, measurements, and optional customer photos.

The app generates a basic mannequin-based preview first, then allows the user to generate an AI-powered gown visualization using the OpenAI Image API.

---

## 1. Project Overview

This application is designed for a custom gown or dress business where customers can:

- Select a gown style
- Select fabric and color
- Enter body measurements
- Upload a front-facing photo
- Preview a mannequin-style visualization
- Generate an AI image preview
- Download or save the generated image

The AI preview is intended for **visualization purposes only** and should not be treated as a final tailoring or fit guarantee.

---

## 2. Technology Stack

### Frontend

- .NET Blazor
- Razor Components
- HTML
- CSS
- JavaScript interop, if needed for preview rendering

### Backend

- C#
- ASP.NET Core
- Entity Framework Core
- REST API endpoints

### Database

- PostgreSQL

### AI Integration

- OpenAI API
- OpenAI Image Model 1.5, or configured image generation model

### Authentication

- Existing OAuth login system
- Existing user authentication flow

### Storage

Recommended storage options:

- Azure Blob Storage
- AWS S3
- Cloudinary
- Local file storage for development only

---

## 3. Core MVP Features

The MVP includes the following functionality:

1. Style and design selection
2. Fabric and color selection
3. Measurement input form
4. Optional customer photo upload
5. Basic mannequin preview
6. AI-generated gown preview
7. Download generated preview image
8. Save generated previews to the database
9. Preview history for customers or staff
10. Basic admin management for gown styles and fabrics

---

## 4. User Flow

The AI Visualizer App allows users to create a gown preview by selecting styles, fabrics, and colors. Fabric and color selections are sourced from a connected third-party B2B marketing platform through an API.

The app does not require the user to manually upload or create fabric options. Instead, approved fabrics, colors, and design references are retrieved from the third-party B2B catalog and displayed inside the visualizer.
