import { Component, OnInit } from '@angular/core';
import { MovieService } from '../../services/movies/movie.service';
import { BookService } from '../../services/books/book.service';
import { AlbumsService } from '../../services/albums/albums.service';
import { GameService } from '../../services/games/game.service';
import { AddModalService } from '../../services/addItemModal/add-modal.service';
import { Album } from '../../models/album.model';
import { Movie } from '../../models/movie.model';
import { Game } from '../../models/game.model';
import { Book } from '../../models/book.model';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  movies: Movie[] = [];
  albums: Album[] = [];
  books: Book[] = [];
  games: Game[] = [];
  recentlyAddedItems: any[] = [];
  allItems: any[] = [];
  filteredItems: any[] = [];
  userId = '';

  firstBookImageUrl: string | null = null;
  firstMovieImageUrl: string | null = null;
  firstAlbumImageUrl: string | null = null;
  firstGameImageUrl: string | null = null;
  firstRecentlyAddedImageUrl: string | null = null;

  showMoviesList = false;
  showBooksList = false;
  showAlbumsList = false;
  showGamesList = false;
  showRecentlyAdded = false;
  isSearching: boolean = false;

  activeList: string | null = null;
  activeSort: string | null = null;
  searchQuery: string | null = null;


  constructor(
    private movieService: MovieService,
    private bookService: BookService,
    private albumService: AlbumsService,
    private gameService: GameService,
    private addModalService: AddModalService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.userId = localStorage.getItem('userId') || '';

    if (this.userId) {
      this.loadBooks();
      this.loadAlbums();
      this.loadGames();
      this.loadMovies();
    } else {
      console.error('User ID not found.');
    }
  }

  openModal(): void {
    this.addModalService.open();
  }

  loadMovies(): void {
    if (!this.userId) return;
    this.movieService.getUserMovies(this.userId).subscribe({
      next: (response) => {
        if (!Array.isArray(response)) {
          console.error('Unexpected Movies API response structure', response);
          return;
        }
        this.movies = response;
        console.log('🎬 Processed Movies:', this.movies);
        this.firstMovieImageUrl = this.movies.length
          ? this.movieService.getPosterUrl(this.movies[0].posterPath)
          : null;

          this.getRecentlyAdded();
      },
      error: (err) => {
        console.error('Error fetching movies:', err);
      },
    });
  }

  loadBooks(): void {
    if (!this.userId) return;

    this.bookService.getUserBooks(this.userId).subscribe({
      next: (response) => {
        if (!Array.isArray(response)) {
          console.error('Unexpected books API response structure', response);
          return;
        }

        this.books = response;
        this.firstBookImageUrl = this.books.length
          ? this.books[0].artwork
          : null;

          this.getRecentlyAdded();
      },
      error: (err) => {
        console.error('Error fetching books:', err);
      },
    });
  }

  loadAlbums(): void {
    if (!this.userId) return;

    this.albumService.getUserAlbums(this.userId).subscribe({
      next: (response) => {
        if (!Array.isArray(response)) {
          console.error('Unexpected Albums API response structure', response);
          return;
        }

        this.albums = response;

        this.firstAlbumImageUrl = this.albums.length
          ? this.albums[0].artwork
          : null;

          this.getRecentlyAdded();
      },
      error: (err) => {
        console.error('Error fetching albums:', err);
      },
    });
  }

  loadGames(): void {
    if (!this.userId) return;

    this.gameService.getUserGames(this.userId).subscribe({
      next: (response) => {
        if (!Array.isArray(response)) {
          console.error('Unexpected Games API response structure', response);
          return;
        }

        this.games = response;

        this.firstGameImageUrl = this.games.length
          ? this.games[0].artwork
          : null;

          this.getRecentlyAdded();
      },
      error: (err) => {
        console.error('Error fetching games:', err);
      },
    });
  }

  removeItemFromList(item: any, type: string) {
    if (!this.userId) {
      console.error('User ID not found.');
      return;
    }

    let itemId = item.apiId;

    if (!itemId) {
      console.error('Item ID not found for', item);
      return;
    }

    let apiUrl = '';

    if (type != 'albums') {
      apiUrl = `http://localhost:5000/api/${type}/${this.userId}/${itemId}`;
    } else {
      apiUrl = `http://localhost:5000/api/music/${this.userId}/${itemId}`;
    }

    this.http.delete(apiUrl, { responseType: 'text' }).subscribe({
      next: () => {
        console.log(`Deleted ${item.title || item.name} from ${type}`);

        switch (type) {
          case 'movies':
            this.movies = this.movies.filter((m) => m.apiId !== itemId);
            break;
          case 'books':
            this.books = this.books.filter((b) => b.apiId !== itemId);
            break;
          case 'albums':
            this.albums = this.albums.filter((a) => a.apiId !== itemId);
            break;
          case 'games':
            this.games = this.games.filter((g) => g.apiId !== itemId);
            break;
        }
      },
      error: (err) => {
        console.error(`Error deleting ${type}:`, err);
      },
    });
  }

  updateRating(item: any, type: string, rating: number) {
    if (!this.userId) {
      console.error('User ID not found.');
      return;
    }

    let itemId = item.apiId;
    let apiUrl = '';

    if (type != 'albums') {
      apiUrl = `http://localhost:5000/api/${type}/${this.userId}/rate/${itemId}`;
    } else {
      apiUrl = `http://localhost:5000/api/music/${this.userId}/rate/${itemId}`;
    }

    const requestBody = { rating };

    this.http.put(apiUrl, requestBody, { responseType: 'text' }).subscribe({
      next: () => {
        console.log(
          `Updated rating for ${item.title || item.name} to ${rating}`
        );

        item.rating = rating;
      },
      error: (err) => {
        console.error(`Error updating rating for ${type}:`, err);
      },
    });
  }

  toggleRating(item: any) {
    item.showRating = !item.showRating;
  }

  setActiveList(listType: string): void {
    this.showMoviesList = listType === 'movies';
    this.showBooksList = listType === 'books';
    this.showAlbumsList = listType === 'albums';
    this.showGamesList = listType === 'games';
    this.showRecentlyAdded = listType === 'recentlyAdded';

    console.log(this.albums);

    this.activeList = listType;
  }

  sortListBy(activeList: string, criteria: 'rating' | 'title' | 'added') {
    if (!this.activeList) return;

    let list: any[] = [];

    switch (this.activeList) {
      case 'movies': list = this.movies; break;
      case 'games': list = this.games; break;
      case 'albums': list = this.albums; break;
      case 'books': list = this.books; break;
    }

    if (list.length === 0) return; 
    // sort
    list.sort((a, b) => { 
      if(criteria === 'rating') return (b.rating ?? 0) - (a.rating ?? 0); 
      if (criteria === 'title') return a.title.localeCompare(b.title);
      if (criteria === 'added') {
        const dateA = new Date(a.addedAt).getTime();
        const dateB = new Date(b.addedAt).getTime();
        return dateB - dateA; // Sort by most recent first
    }
      return 0;
    });

    console.log(`After sorting by ${criteria}:`, list.map(item => ({ title: item.title, addedAt: item.addedAt })));

  }

  getRecentlyAdded(): void {
    this.allItems = [
      ...this.movies.map((item) => ({ ...item, type: "movies" })),
      ...this.books.map((item) => ({ ...item, type: "books" })),
      ...this.albums.map((item) => ({ ...item, type: "albums" })),
      ...this.games.map((item) => ({ ...item, type: "games" })),
    ]; // all in one list with type assigned
  
    this.recentlyAddedItems = this.allItems
      .filter(i => i.addedAt)
      .sort((a, b) => new Date(b.addedAt).getTime() - new Date(a.addedAt).getTime())
      .slice(0, 20);
  
    if (this.recentlyAddedItems.length > 0) {
      const randomIndex = Math.floor(Math.random() * this.recentlyAddedItems.length);
      this.firstRecentlyAddedImageUrl = this.recentlyAddedItems[randomIndex].artwork || this.recentlyAddedItems[randomIndex].posterPath || null;
    } else {
      this.firstRecentlyAddedImageUrl = null;
    }
  }

  searchUserItems(): void {
    if (!this.searchQuery?.trim()) {
      this.filteredItems = []; // reset search results
      this.isSearching = false; // show normal
      return;
    }
  
    const lower = this.searchQuery.toLowerCase();
  
    this.filteredItems = this.allItems.filter(i =>
      i.title.toLowerCase().includes(lower) || 
      (i.artist && i.artist.toLowerCase().includes(lower)) ||
      (i.authors && i.authors.toLowerCase().includes(lower))
    );
  
    this.isSearching = true; // so we can show no res
  }

  getPosterUrl(posterPath: string): string {
    return posterPath
      ? `https://image.tmdb.org/t/p/w200${posterPath}`
      : 'https://via.placeholder.com/150';
  }

  // formatting helpersr
  formatDate(dateString: string): string { 
    if (!dateString) return "Unknown Date";

    const date = new Date(dateString); 
    return date.toLocaleDateString("en-US", { month: "short", day: "2-digit", year: "numeric" });
  }

  formatItemType(type: string): string {
    if (!type) return "Unknown Type";
    return type.charAt(0).toUpperCase() + type.slice(1);
  }
}
