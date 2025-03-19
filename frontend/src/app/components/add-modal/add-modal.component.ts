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
  userId: string = '';
  successMessage: string = '';

  selectedItem: any | null = null; // Stores the item user clicked on

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

  // Handles search request
  onSearch() {
    if (!this.searchQuery.trim()) return;

    switch (this.selectedType) {
      case 'movie':
        this.movieService.searchMovie(this.searchQuery).subscribe({
          next: (results) => (this.searchResults = results),
          error: (err) => {
            console.error('Movie search failed:', err);
            this.searchResults = [];
          },
        });
        break;

      case 'book':
        this.bookService.searchBook(this.searchQuery).subscribe({
          next: (results) => (this.searchResults = results),
          error: (err) => {
            console.error('Book search failed:', err);
            this.searchResults = [];
          },
        });
        break;

      case 'album':
        this.albumService.searchAlbums(this.searchQuery).subscribe({
          next: (results) => (this.searchResults = results),
          error: (err) => {
            console.error('Album search failed:', err);
            this.searchResults = [];
          },
        });
        break;

      case 'game':
        this.gameService.searchGames(this.searchQuery).subscribe({
          next: (results) => (this.searchResults = results),
          error: (err) => {
            console.error('Game search failed:', err);
            this.searchResults = [];
          },
        });
        break;

      default:
        console.warn('Invalid search type:', this.selectedType);
        this.searchResults = [];
        break;
    }
  }

  // Handles selecting an item for more details
  selectItem(item: any) {
    this.selectedItem = {
      id: item.id,
      title: item.title || item.name || 'Untitled',
      image:
        item.poster_path ||
        item.artwork ||
        item.image ||
        item.thumbnail ||
        'https://via.placeholder.com/150',
      description: item.description || 'No description available.',
      rating: item.rating !== 'N/A' ? item.rating + '/5' : 'Not Rated',
      type: this.selectedType,
      extraInfo: this.getExtraInfo(item),
    };
  }

  getExtraInfo(item: any): string {
    switch (this.selectedType) {
      case 'movie':
        return `⭐ ${item.rating}/5`;
      case 'book':
        return `${item.pageCount} pages | Published: ${item.publishedDate}`;
      case 'album':
        return `Artist: ${item.artist}`;
      case 'game':
        return `Release: ${item.releaseDate}`;
      default:
        return '';
    }
  }

  // Handles returning to search results
  backToSearch() {
    this.selectedItem = null;
  }

  addItemToList(item: any) {
    if (!this.userId) {
        console.error('User ID not found.');
        return;
    }

    let apiUrl = this.selectedType !== 'album'
        ? `http://localhost:5000/api/${this.selectedType}s/add`
        : `http://localhost:5000/api/music/add`;

    const requestBody: any = {
        userId: this.userId,
    };

    if (item.id && item.id.trim() !== '') {
        requestBody.itemId = item.id;
    } else {
        requestBody.title = item.title || item.name || 'Unknown Title';
        
        // ✅ If artist is missing, extract it from extraInfo
        if (item.artist && item.artist.trim() !== "" && item.artist !== "Unknown Artist") {
            requestBody.artist = item.artist.trim();
        } else if (item.extraInfo?.startsWith("Artist: ")) {
            requestBody.artist = item.extraInfo.replace("Artist: ", "").trim();
        } else {
            requestBody.artist = "DEBUG_MISSING_ARTIST";
        }
    }

    this.http.post(apiUrl, requestBody).subscribe({
      next: () => {
        this.successMessage = `'${
          item.title || item.name
        }' added successfully!`;
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
