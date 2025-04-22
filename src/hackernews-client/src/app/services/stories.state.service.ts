import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Story } from './hacker-news.service';
import { map } from 'rxjs/operators';

export interface StoriesState {
  stories: Story[];
  selectedStory: Story | null;
  loading: boolean;
  error: any;
}

const initialState: StoriesState = {
  stories: [],
  selectedStory: null,
  loading: false,
  error: null,
};

@Injectable({
  providedIn: 'root',
})
export class StoriesStateService {
  private state = new BehaviorSubject<StoriesState>(initialState);
  state$ = this.state.asObservable();

  getStories(): Observable<Story[]> {
    return this.state$.pipe(map((state) => state.stories));
  }

  getLoading(): Observable<boolean> {
    return this.state$.pipe(map((state) => state.loading));
  }

  getError(): Observable<any> {
    return this.state$.pipe(map((state) => state.error));
  }

  setLoading(loading: boolean): void {
    this.updateState({ loading, error: null });
  }

  setStories(stories: Story[]): void {
    this.updateState({ stories, loading: false, error: null });
  }

  setError(error: any): void {
    this.updateState({ error, loading: false });
  }

  private updateState(partialState: Partial<StoriesState>): void {
    this.state.next({
      ...this.state.value,
      ...partialState,
    });
  }
}
