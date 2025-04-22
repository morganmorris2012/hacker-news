import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { StoryListComponent } from './story-list.component';
import { HackerNewsService } from '../../services/hacker-news.service';
import { StoriesStateService } from '../../services/stories.state.service';

describe('StoryListComponent', () => {
  let component: StoryListComponent;
  let fixture: ComponentFixture<StoryListComponent>;
  let hackerNewsService: jasmine.SpyObj<HackerNewsService>;
  let stateService: jasmine.SpyObj<StoriesStateService>;

  const mockStories = [
    {
      id: 1,
      title: 'Test Story 1',
      url: 'http://test1.com',
      by: 'user1',
      score: 100,
      time: 1234567890,
      descendants: 10,
      type: 'story',
    },
    {
      id: 2,
      title: 'Test Story 2',
      url: 'http://test2.com',
      by: 'user2',
      score: 200,
      time: 1234567890,
      descendants: 20,
      type: 'story',
    },
  ];

  beforeEach(async () => {
    const hackerNewsSpy = jasmine.createSpyObj('HackerNewsService', [
      'getNewestStories',
      'searchStories',
    ]);
    hackerNewsSpy.getNewestStories.and.returnValue(of(mockStories));
    hackerNewsSpy.searchStories.and.returnValue(of(mockStories));

    const stateSpy = jasmine.createSpyObj('StoriesStateService', [
      'getStories',
      'getLoading',
      'getError',
      'setStories',
      'setLoading',
      'setError',
    ]);
    stateSpy.getStories.and.returnValue(of(mockStories));
    stateSpy.getLoading.and.returnValue(of(false));
    stateSpy.getError.and.returnValue(of(null));

    await TestBed.configureTestingModule({
      imports: [CommonModule, ReactiveFormsModule, StoryListComponent],
      providers: [
        { provide: HackerNewsService, useValue: hackerNewsSpy },
        { provide: StoriesStateService, useValue: stateSpy },
      ],
    }).compileComponents();

    hackerNewsService = TestBed.inject(
      HackerNewsService
    ) as jasmine.SpyObj<HackerNewsService>;
    stateService = TestBed.inject(
      StoriesStateService
    ) as jasmine.SpyObj<StoriesStateService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(StoryListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load stories on init', () => {
    expect(hackerNewsService.getNewestStories).toHaveBeenCalledWith(1, 20);
    expect(stateService.setStories).toHaveBeenCalledWith(mockStories);
  });

  it('should search stories when search input changes', fakeAsync(() => {
    component.searchControl.setValue('test');
    tick(300); // Wait for debounceTime
    fixture.detectChanges();

    expect(hackerNewsService.searchStories).toHaveBeenCalledWith('test', 1, 20);
    expect(stateService.setStories).toHaveBeenCalledWith(mockStories);
  }));

  it('should change page when page button is clicked', () => {
    component.onPageChange(2);
    expect(component.currentPage).toBe(2);
    expect(hackerNewsService.getNewestStories).toHaveBeenCalledWith(2, 20);
  });
});
