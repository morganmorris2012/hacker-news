import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { HackerNewsService, Story } from '../../services/hacker-news.service';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { StoriesStateService } from '../../services/stories.state.service';

@Component({
  selector: 'app-story-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe],
  templateUrl: './story-list.component.html',
  styleUrls: ['./story-list.component.scss'],
})
export class StoryListComponent implements OnInit {
  stories$: Observable<Story[]>;
  isLoading$: Observable<boolean>;
  error$: Observable<any>;
  currentPage = 1;
  pageSize = 20;
  searchControl = new FormControl('');

  constructor(
    private hackerNewsService: HackerNewsService,
    private stateService: StoriesStateService
  ) {
    this.stories$ = this.stateService.getStories();
    this.isLoading$ = this.stateService.getLoading();
    this.error$ = this.stateService.getError();
  }

  ngOnInit(): void {
    this.loadStories();

    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) => {
          this.currentPage = 1;
          return query
            ? this.hackerNewsService.searchStories(
                query,
                this.currentPage,
                this.pageSize
              )
            : this.hackerNewsService.getNewestStories(
                this.currentPage,
                this.pageSize
              );
        })
      )
      .subscribe({
        next: (stories) => this.stateService.setStories(stories),
        error: (error) => this.stateService.setError(error),
      });
  }

  loadStories(): void {
    this.stateService.setLoading(true);

    const observable = this.searchControl.value
      ? this.hackerNewsService.searchStories(
          this.searchControl.value,
          this.currentPage,
          this.pageSize
        )
      : this.hackerNewsService.getNewestStories(
          this.currentPage,
          this.pageSize
        );

    observable.subscribe({
      next: (stories) => this.stateService.setStories(stories),
      error: (error) => this.stateService.setError(error),
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadStories();
  }
}
