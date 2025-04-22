import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoryListComponent } from './components/story-list/story-list.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, StoryListComponent],
  template: `
    <div class="app-container">
      <header class="app-header">
        <div class="header-content">
          <h1>Hacker News Reader</h1>
        </div>
      </header>
      <main class="main-content">
        <app-story-list></app-story-list>
      </main>
      <footer class="app-footer">
        <p>© 2024 Hacker News Reader. All rights reserved.</p>
      </footer>
    </div>
  `,
  styles: [
    `
      .app-container {
        min-height: 100vh;
        background-color: #f8f9fa;
        display: flex;
        flex-direction: column;
      }

      .app-header {
        background-color: #ff6600;
        padding: 1rem 2rem;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        position: sticky;
        top: 0;
        z-index: 100;
      }

      .header-content {
        max-width: 1200px;
        margin: 0 auto;
        display: flex;
        justify-content: center;
        align-items: center;
      }

      h1 {
        margin: 0;
        font-size: 1.5rem;
        font-weight: 600;
        color: white;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto,
          Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
      }

      .main-content {
        flex: 1;
        padding: 2rem;
        max-width: 1200px;
        margin: 0 auto;
        width: 100%;
      }

      .app-footer {
        background-color: #f1f3f4;
        padding: 1rem 2rem;
        text-align: center;
        color: #5f6368;
        font-size: 0.875rem;
      }

      @media (max-width: 768px) {
        .header-content {
          text-align: center;
        }

        .main-content {
          padding: 1rem;
        }
      }
    `,
  ],
})
export class AppComponent {}
