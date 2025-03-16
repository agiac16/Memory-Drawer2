import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Game } from '../../models/game.model';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private apiUrl = 'http://localhost:5000/api/games';
  private searchUrl = 'http://localhost:5000/api/games/search';

  constructor(private http: HttpClient) {}

  getUserGames(userId: string): Observable<{ success: boolean; data: Game[] }> {
    return this.http.get<{ success: boolean; data: Game[] }>(
      `${this.apiUrl}/${userId}/all`
    );
  }

  searchGames(searchQuery: string): Observable<any[]> {
    const apiUrl = `http://localhost:5000/api/games/search`;
    // handled in backend
    return this.http
      .get<any>(apiUrl, {
        params: { title: searchQuery },
        withCredentials: true,
      })
      .pipe(
        map((response: any) => {
          console.log('api res', response);

          if (!response.results) return [];

          return response.results.map((game: any) => ({
            id: game.id,
            title: game.name ?? 'Untitled',
            artwork: game.image?.medium_url
              ? game.image.medium_url // Use artwork directly
              : 'https://via.placeholder.com/150',
          }));
        }),
        catchError((error) => {
          console.error('error searching games', error);
          return [];
        })
      );
  }

  getFirstGameImageUrl(userId: string): Observable<string | null> {
    return new Observable((observer) => {
      this.getUserGames(userId).subscribe((response) => {
        const games = response.data;
        if (games.length > 0) {
          observer.next(games[0].artwork);
        } else {
          observer.next(null);
        }
        observer.complete();
      });
    });
  }
}
