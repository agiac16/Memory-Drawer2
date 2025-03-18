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
      `${this.apiUrl}/${userId}/all`
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

          if (!response.items || response.items.length === 0) {
            console.warn('No books found');
            return [];
          }

          return response.items.map((book: any) => ({
            id: book.id,
            title: book.volumeInfo?.title ?? 'Untitled',
            authors: book.volumeInfo?.authors?.join(', ') ?? 'Unknown Author',
            thumbnail:
              book.volumeInfo?.imageLinks?.thumbnail ??
              'https://via.placeholder.com/150',
            description:
              book.volumeInfo?.description ?? 'No description available.',
            rating: book.volumeInfo?.averageRating ?? 'Not Rated',
            publishedDate: book.volumeInfo?.publishedDate
              ? new Date(book.volumeInfo.publishedDate).toLocaleDateString(
                  'en-US',
                  { month: 'short', day: '2-digit', year: 'numeric' }
                )
              : 'Unknown Date',
            pageCount: book.volumeInfo?.pageCount ?? 'N/A',
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
          observer.next(books[0].artwork);
        } else {
          observer.next(null);
        }
        observer.complete();
      });
    });
  }
}
