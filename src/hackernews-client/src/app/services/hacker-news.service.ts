import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Story {
  id: number;
  title: string;
  url?: string;
  by: string;
  score: number;
  time: number;
  descendants: number;
  type: string;
}

@Injectable({
  providedIn: 'root',
})
export class HackerNewsService {
  private apiUrl = `${environment.apiBaseUrl}/stories`;

  constructor(private http: HttpClient) {}

  getNewestStories(
    page: number = 1,
    pageSize: number = 20
  ): Observable<Story[]> {
    return this.http.get<Story[]>(
      `${this.apiUrl}/newest?page=${page}&pageSize=${pageSize}`
    );
  }

  searchStories(
    query: string,
    page: number = 1,
    pageSize: number = 20
  ): Observable<Story[]> {
    return this.http.get<Story[]>(
      `${this.apiUrl}/search?query=${encodeURIComponent(
        query
      )}&page=${page}&pageSize=${pageSize}`
    );
  }

  getStoryById(id: number): Observable<Story> {
    return this.http.get<Story>(`${this.apiUrl}/${id}`);
  }
}
