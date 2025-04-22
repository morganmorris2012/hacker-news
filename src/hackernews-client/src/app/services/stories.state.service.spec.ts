import { TestBed } from '@angular/core/testing';
import { StoriesStateService, StoriesState } from './stories.state.service';
import { Story } from './hacker-news.service';
import { take } from 'rxjs/operators';

describe('StoriesStateService', () => {
  let service: StoriesStateService;
  const mockStory: Story = {
    id: 1,
    title: 'Test Story',
    by: 'Test User',
    score: 100,
    time: 1234567890,
    descendants: 10,
    type: 'story',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [StoriesStateService],
    });

    service = TestBed.inject(StoriesStateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with default state', () => {
    service.state$.pipe(take(1)).subscribe((state) => {
      expect(state).toEqual({
        stories: [],
        selectedStory: null,
        loading: false,
        error: null,
      });
    });
  });

  it('should get stories', () => {
    const stories = [mockStory];
    service.setStories(stories);

    service
      .getStories()
      .pipe(take(1))
      .subscribe((result) => {
        expect(result).toEqual(stories);
      });
  });

  it('should get loading state', () => {
    service.setLoading(true);

    service
      .getLoading()
      .pipe(take(1))
      .subscribe((loading) => {
        expect(loading).toBe(true);
      });
  });

  it('should get error state', () => {
    const error = new Error('Test error');
    service.setError(error);

    service
      .getError()
      .pipe(take(1))
      .subscribe((result) => {
        expect(result).toEqual(error);
      });
  });

  it('should set loading state and clear error', () => {
    service.setError(new Error('Previous error'));
    service.setLoading(true);

    service.state$.pipe(take(1)).subscribe((state) => {
      expect(state.loading).toBe(true);
      expect(state.error).toBeNull();
    });
  });

  it('should set stories and clear loading and error', () => {
    const stories = [mockStory];
    service.setLoading(true);
    service.setError(new Error('Previous error'));
    service.setStories(stories);

    service.state$.pipe(take(1)).subscribe((state) => {
      expect(state.stories).toEqual(stories);
      expect(state.loading).toBe(false);
      expect(state.error).toBeNull();
    });
  });

  it('should set error and clear loading', () => {
    const error = new Error('Test error');
    service.setLoading(true);
    service.setError(error);

    service.state$.pipe(take(1)).subscribe((state) => {
      expect(state.error).toEqual(error);
      expect(state.loading).toBe(false);
    });
  });
});
