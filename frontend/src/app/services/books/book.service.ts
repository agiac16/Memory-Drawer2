import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Book } from '../../models/book.model';
import { environment } from '../../../enviroments/enviroment';

@Injectable({
  providedIn: 'root'
})
export class BookService {
  private apiUrl = 'http://localhost:5050/api/books'; 
  private booksApiUrl = 'https://www.googleapis.com/books/v1/volumes'
  private apiKey = "";

  constructor(private http: HttpClient) {}

  getUserBooks(userId: string): Observable<{ success: boolean; data: Book[] }> {
    return this.http.get<{ success: boolean; data: Book[] }>(`${this.apiUrl}/all`, {
      params: { userId }
    });
  }

  searchBook(searchName: string): Observable<any[]> {
    return this.http.get<{ items?: any[]}>(this.booksApiUrl, {
      params: { 
        q: searchName,
        key: this.apiKey
      }
    }).pipe(
      map(response => response.items || [])
    )
  }


  getFirstBookImageUrl(userId: string): Observable<string | null> {
    return new Observable(observer => {
      this.getUserBooks(userId).subscribe(response => {
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