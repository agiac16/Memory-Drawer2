import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Game } from '../../models/game.model';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private apiUrl = 'http://localhost:5050/api/games'; 
  private searchUrl = "http://localhost:5050/api/games/search"

  constructor(private http: HttpClient) { }

  getUserGames(userId: string): Observable<{ success: boolean; data: Game[] }> {
    return this.http.get<{ success: boolean; data: Game[] }>(`${this.apiUrl}/all`, {
      params: { userId }
    });
  }

  searchGames(searchName: string): Observable<any[]> {
    return this.http.get<{ results?: any[] }>(this.searchUrl, {
      params: { query: searchName }
    }).pipe(
      map(response => response.results || [])
    );
  }

  getFirstGameImageUrl(userId: string): Observable<string | null> {
    return new Observable(observer => {
      this.getUserGames(userId).subscribe(response => {
        const games = response.data;
        if (games.length > 0) {
          observer.next(games[0].cover);
        } else {
          observer.next(null);
        }
        observer.complete();
      });
    });
  }
}
