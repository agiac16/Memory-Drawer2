import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book } from '../../models/book.model';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../enviroments/enviroment';

@Injectable({
  providedIn: 'root',
})
export class BookService {
  private apiUrl = 'http://localhost:5000/api/books';
  private booksApiUrl = 'https://www.googleapis.com/books/v1/volumes';
  private apiKey = '';

  constructor(private http: HttpClient) {}

  getUserBooks(userId: string): Observable<{ success: boolean; data: Book[] }> {
    return this.http.get<{ success: boolean; data: Book[] }>(
      `${this.apiUrl}/all`,
      {
        params: { userId },
      }
    );
  }

  searchBook(searchQuery: string): Observable<any[]> {
    const apiUrl = `http://localhost:5000/api/books/search`;

    return this.http
      .get<any>(apiUrl, {
        params: { title: searchQuery },
        withCredentials: true,
      })
      .pipe(
        map((response: any) => {
          console.log('API Response:', response);

          // ✅ Ensure `items` exist before mapping
          if (!response.items || response.items.length === 0) {
            console.warn('No books found');
            return [];
          }

          return response.items.map((book: any) => ({
            title: book.volumeInfo?.title ?? 'Untitled',
            authors: book.volumeInfo?.authors?.join(', ') ?? 'Unknown Author',
            thumbnail: book.volumeInfo?.imageLinks?.thumbnail
              ? book.volumeInfo.imageLinks.thumbnail
              : 'https://via.placeholder.com/150',
          }));
        }),
        catchError((error) => {
          console.error('Error fetching books:', error);
          return [];
        })
      );
  }

  getFirstBookImageUrl(userId: string): Observable<string | null> {
    return new Observable((observer) => {
      this.getUserBooks(userId).subscribe((response) => {
        const books = response.data;
        if (books.length > 0) {
          observer.next(books[0].cover);
        } else {
          observer.next(null);
        }
        observer.complete();
      });
    });
  }
}
