import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { HackerNewsService, Story } from './hacker-news.service';
import { environment } from '../../environments/environment';

describe('HackerNewsService', () => {
  let service: HackerNewsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [HackerNewsService],
    });

    service = TestBed.inject(HackerNewsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch newest stories', () => {
    const mockStories: Story[] = [
      {
        id: 1,
        title: 'Test Story',
        by: 'Test User',
        score: 100,
        time: 1234567890,
        descendants: 10,
        type: 'story',
      },
    ];

    service.getNewestStories(1, 20).subscribe((stories) => {
      expect(stories).toEqual(mockStories);
    });

    const req = httpMock.expectOne(
      `${environment.apiBaseUrl}/stories/newest?page=1&pageSize=20`
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockStories);
  });

  it('should search stories', () => {
    const mockStories: Story[] = [
      {
        id: 1,
        title: 'Search Result',
        by: 'Test User',
        score: 100,
        time: 1234567890,
        descendants: 10,
        type: 'story',
      },
    ];

    const searchQuery = 'test query';
    service.searchStories(searchQuery, 1, 20).subscribe((stories) => {
      expect(stories).toEqual(mockStories);
    });

    const req = httpMock.expectOne(
      `${environment.apiBaseUrl}/stories/search?query=${encodeURIComponent(
        searchQuery
      )}&page=1&pageSize=20`
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockStories);
  });

  it('should get story by id', () => {
    const mockStory: Story = {
      id: 1,
      title: 'Single Story',
      by: 'Test User',
      score: 100,
      time: 1234567890,
      descendants: 10,
      type: 'story',
    };

    service.getStoryById(1).subscribe((story) => {
      expect(story).toEqual(mockStory);
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/stories/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockStory);
  });
});
