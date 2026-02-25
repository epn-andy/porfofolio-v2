# Portfolio Website & Custom CMS Development Plan

## 1. Unique UI/UX Concept: "The Automated Pipeline"
* **Visual Metaphor**: The website structure mimics a logic flow or DevOps pipeline. Sections are connected by a continuous SVG line that animates as the user scrolls.
* **Light Mode (Default)**: Crisp white background, thin slate-grey lines, distinct black text. Feels like a technical architectural blueprint.
* **Dark Mode**: Deep navy/black background, glowing subtle accent colors (like syntax highlighting in an IDE). Feels like a secure terminal.
* **Interactivity**: Hovering over projects or job histories "executes" a node, slightly expanding it and highlighting the path that led to it.

## 2. Tech Stack & Tooling
* **Frontend**: Vue 3 (Composition API, `<script setup>`), Vite, Tailwind CSS, Vue Router, Pinia (for state management).
* **Backend**: ASP.NET Core Web API (.NET 8), Entity Framework Core.
* **Database**: PostgreSQL.
* **Authentication**: JWT stored securely.

## 3. Best Practices & Security Implementation

### Frontend Security & Best Practices (Vue.js)
* **Cross-Site Scripting (XSS) Prevention**: Since articles are written in Markdown and rendered as HTML, **never** use `v-html` directly on raw user input. Use a sanitizer like `DOMPurify` before rendering the Markdown into the DOM to strip malicious scripts.
* **State Management**: Use Pinia instead of Vuex for better TypeScript support and modularity.
* **Route Guards**: Implement global navigation guards (`router.beforeEach`) to protect all `/admin` routes, checking for valid authentication state before rendering the view.
* **Lazy Loading**: Use dynamic imports (`const AdminView = () => import('./views/AdminView.vue')`) for routes to split the bundle and improve initial load times.

### Backend Security & Best Practices (.NET Core)
* **Authentication & Token Storage**: Do not store JWTs in `localStorage` as they are vulnerable to XSS. Configure the .NET API to issue the JWT in an **HttpOnly, Secure, SameSite=Strict cookie**.
* **Password Hashing**: Never store plain text passwords. Use the `BCrypt.Net-Next` library to hash admin passwords before saving them to PostgreSQL.
* **Cross-Origin Resource Sharing (CORS)**: Strictly configure CORS in `Program.cs`. Only allow requests from your specific production frontend URL (no `AllowAnyOrigin` in production).
* **Rate Limiting**: Implement the built-in .NET 8 Rate Limiting middleware on the login endpoint and public API endpoints to prevent brute-force attacks and DDoS.
* **SQL Injection Prevention**: Entity Framework Core automatically parameterizes queries, protecting against SQL injection. Avoid writing raw SQL queries using string interpolation.
* **Global Exception Handling**: Implement a custom middleware to catch unhandled exceptions. Return standardized JSON error responses without exposing stack traces in production.

## 4. Phased Execution

### Phase 1: Infrastructure & API Foundation
1.  Initialize ASP.NET Core API and PostgreSQL database.
2.  Implement EF Core models and apply the first migration.
3.  Set up the global exception handler and strict CORS policy.
4.  Implement Auth endpoints (Login) returning HttpOnly cookies using BCrypt for password verification.

### Phase 2: Content Management API
1.  Build RESTful controllers for Articles, Projects, and JobHistory.
2.  Apply `[Authorize]` attributes to POST/PUT/DELETE endpoints.
3.  Add .NET Rate Limiting to the GET endpoints.

### Phase 3: The "Pipeline" Frontend
1.  Scaffold Vue 3 + Vite + Tailwind CSS.
2.  Build the custom SVG-based "pipeline" layout component that tracks scroll position.
3.  Develop Light/Dark mode toggles using Tailwind's class strategy.
4.  Integrate public-facing views with backend GET endpoints.

### Phase 4: Secure Admin Dashboard
1.  Build the login interface and integrate with the backend to handle the HttpOnly cookie.
2.  Set up Vue Router guards to protect the `/admin` dashboard.
3.  Implement CRUD interfaces for the database models.
4.  Integrate `marked` (for Markdown parsing) and `DOMPurify` (for XSS sanitization) in the Article editor.