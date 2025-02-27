import { Injectable, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Movie } from '../../models/movie.model';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class MovieService {
  private apiUrl = 'http://localhost:5050/api/movies'; 
  private movieApiUrl = 'https://api.themoviedb.org/3/search';

  constructor(private http: HttpClient) {}

  getUserMovies(userId: string): Observable<{ success: boolean; data: Movie[] }> {
    return this.http.get<{ success: boolean; data: Movie[] }>(`${this.apiUrl}/all`, { params: { userId } });
  }

  getPosterUrl(posterPath: string): string {
    const baseUrl = 'https://image.tmdb.org/t/p/w500';
    return posterPath ? `${baseUrl}${posterPath}` : 'https://via.placeholder.com/150';
  }

  searchMovie(searchQuery: string): Observable<any[]> {
    return this.http.get<any[]>(`http://localhost:5050/api/movies/search`, {
        params: { movieName: searchQuery },
        withCredentials: true
    }).pipe(
        map((response: any[]) => {
            console.log("🟢 Corrected API Response:", response);
            return response.map(movie => ({
                title: movie.title ?? 'Untitled',
                poster_path: movie.poster_path 
                    ? `https://image.tmdb.org/t/p/w200${movie.poster_path}` 
                    : 'https://via.placeholder.com/150',
            }));
        }),
        catchError(error => {
            console.error("❌ Error searching movies:", error);
            return [];
        })
    );
  }

  getFirstMovieImageUrl(userId: string): Observable<string | null> {
    return new Observable(observer => {
        this.getUserMovies(userId).subscribe({
            next: (response) => {
                const movies = response?.data;  
                if (movies && movies.length > 0) {
                    const posterUrl = this.getPosterUrl(movies[0].poster_path);
                    observer.next(posterUrl);
                } else {
                    observer.next(null);
                }
                observer.complete();
            },
            error: (err) => {
                console.error('Error fetching movies:', err);
                observer.next(null);
                observer.complete();
            }
        });
    });
}}
