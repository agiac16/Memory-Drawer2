import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AddModalService } from '../../services/addItemModal/add-modal.service';
import { MovieService } from '../../services/movies/movie.service';
import { BookService } from '../../services/books/book.service';
import { AlbumsService } from '../../services/albums/albums.service';
import { GameService } from '../../services/games/game.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-add-modal',
  templateUrl: './add-modal.component.html',
  styleUrl: './add-modal.component.css',
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class AddModalComponent {
  selectedType: string = '';
  searchQuery: string = '';
  searchResults: any[] = [];
  userId: string = ''; // Fetch user ID from localStorage
  successMessage: string = ''; // Display success message after adding

  types = [
    { value: 'movie', label: 'Movie' },
    { value: 'book', label: 'Book' },
    { value: 'album', label: 'Album' },
    { value: 'game', label: 'Game' },
  ];

  constructor(
    private movieService: MovieService,
    private albumService: AlbumsService,
    private bookService: BookService,
    private gameService: GameService,
    private modalService: AddModalService,
    private http: HttpClient
  ) {}

  ngOnInit() {
    this.userId = localStorage.getItem('userId') || '';
  }

  onSearch() { 
    if (!this.searchQuery.trim()) return; 

    switch (this.selectedType) {
      case 'movie':
        this.movieService.searchMovie(this.searchQuery).subscribe({
          next: (results) => this.searchResults = results,
          error: (err) => {
            console.error('Movie search failed:', err);
            this.searchResults = [];
          }
        });
        break;

      case 'book':
        this.bookService.searchBook(this.searchQuery).subscribe({
          next: (results) => this.searchResults = results,
          error: (err) => {
            console.error('Book search failed:', err);
            this.searchResults = [];
          }
        });
        break;

      case 'album':
        this.albumService.searchAlbums(this.searchQuery).subscribe({
          next: (results) => this.searchResults = results,
          error: (err) => {
            console.error('Album search failed:', err);
            this.searchResults = [];
          }
        });
        break;

      case 'game':
        this.gameService.searchGames(this.searchQuery).subscribe({
          next: (results) => this.searchResults = results,
          error: (err) => {
            console.error('Game search failed:', err);
            this.searchResults = [];
          }
        });
        break;

      default:
        console.warn('Invalid search type:', this.selectedType);
        this.searchResults = [];
        break;
    }
  }

  addItemToList(item: any) {
    if (!this.userId) {
      console.error("User ID not found.");
      return;
    }
    const apiUrl = ""

    if (this.selectedType !== "music") { 
      const apiUrl = `http://localhost:5000/api/${this.selectedType}s/add`;
    } else { 
      // handle music bcs no (s) 
      const apiUrl = `http://localhost:5000/api/${this.selectedType}/add`;
    }
    
  
    const requestBody = {
      userId: this.userId,
      itemId: String(item.id)
    };
    
    if (!requestBody.itemId) {
      console.error("itemId is missing from the request!", item);
      return;
    }
  
    this.http.post(apiUrl, requestBody).subscribe({
      next: () => {
        this.successMessage = `'${item.title || item.name}' added successfully!`;
        setTimeout(() => (this.successMessage = ''), 3000);
      },
      error: (err) => {
        console.error(`Error adding ${this.selectedType}:`, err);
      },
    });
  }

  close() {
    this.modalService.close();
  }

  onTypeSelect(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.selectedType = target.value;
  }
}
