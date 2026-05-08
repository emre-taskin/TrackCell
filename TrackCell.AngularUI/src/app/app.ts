import { Component, computed, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { ToastContainerComponent } from './shared/toast-container.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastContainerComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('TrackCell');
  private readonly fullscreen = signal(false);
  readonly isFullscreen = computed(() => this.fullscreen());

  constructor(private router: Router, private route: ActivatedRoute) {
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => {
      let r = this.route.firstChild;
      while (r?.firstChild) r = r.firstChild;
      this.fullscreen.set(!!r?.snapshot.data?.['fullscreen']);
      const t = r?.snapshot.data?.['title'];
      if (t) document.title = `${t} - TrackCell MES`;
    });
  }
}
