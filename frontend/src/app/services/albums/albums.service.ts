import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Album } from '../../models/album.model';

@Injectable({
  providedIn: 'root',
})
export class AlbumsService {
  private apiUrl = 'http://localhost:5000/api/music';
  private lastFmApiUrl = 'http://ws.audioscrobbler.com/2.0/';

  constructor(private http: HttpClient) {}

  getUserAlbums(
    userId: string
  ): Observable<{ success: boolean; data: Album[] }> {
    return this.http.get<{ success: boolean; data: Album[] }>(
      `${this.apiUrl}/${userId}/all`
    );
  }

  // handled on backend
  searchAlbums(searchQuery: string): Observable<any[]> {
    const apiUrl = `http://localhost:5000/api/music/search`;
  
    return this.http.get<any>(apiUrl, {
      params: { title: searchQuery },
      withCredentials: true,
    }).pipe(
      map((response: any) => {
        console.log("API Response:", response);
  
        if (!response.results || !response.results.albummatches || !response.results.albummatches.album) {
          console.warn("No Music found");
          return [];
        }
  
        return response.results.albummatches.album.map((album: any) => ({
          id: album.mbid,
          title: album.name ?? "Untitled",
          artist: album.artist ?? "Unknown Artist",
          image: album.image?.find((img: any) => img.size === "extralarge")?.['#text']  // get largest
            || album.image?.find((img: any) => img.size === "large")?.['#text']
            || 'https://via.placeholder.com/150', // Fallback image
        }));
        
      }),
      catchError((error) => {
        console.error("Error fetching albums:", error);
        return [];
      })
    );
  }

  getFirstAlbumImageUrl(userId: string): Observable<string | null> {
    return new Observable((observer) => {
      this.getUserAlbums(userId).subscribe((response) => {
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
