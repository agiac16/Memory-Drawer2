import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Album } from '../../models/album.model';

@Injectable({
  providedIn: 'root'
})
export class AlbumsService {
  private apiUrl = 'http://localhost:5000/api/music'; 
  private lastFmApiUrl = 'http://ws.audioscrobbler.com/2.0/'
  
  constructor(private http: HttpClient) {}

  getUserAlbums(userId: string): Observable<{ success: boolean; data: Album[] }> {
    return this.http.get<{ success: boolean; data: Album[] }>(`${this.apiUrl}/all`, {
      params: { userId }
    });
  }

  // handled on backend 
  searchAlbums(searchQuery: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/search`, {
      params: { albumName: searchQuery }, 
      withCredentials: true
    }).pipe(
      map(response => {
        console.log("search results", response);
        return response || [] // results or an empty array
      }), 
      catchError(error => {
        console.error("Error searching albums:", error);
        throw error;
      })
    );
  }

  getFirstAlbumImageUrl(userId: string): Observable<string | null> {
    return new Observable(observer => {
      this.getUserAlbums(userId).subscribe(response => {
        const albums = response.data;
        if (albums.length > 0) {
          observer.next(albums[0].artwork);
        } else {
          observer.next(null);
        }
        observer.complete();
      });
    });
  }
}