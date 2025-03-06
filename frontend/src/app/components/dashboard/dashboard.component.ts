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

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  movies: Movie[] = [];
  albums: Album[] = [];
  books: Book[] = [];
  games: Game[] = [];
  userId = '';

  firstBookImageUrl: string | null = null;
  firstMovieImageUrl: string | null = null;
  firstAlbumImageUrl: string | null = null;
  firstGameImageUrl: string | null = null;

  showMoviesList = false;
  showBooksList = false;
  showAlbumsList = false;
  showGamesList = false;

  constructor(
    private movieService: MovieService,
    private bookService: BookService,
    private albumService: AlbumsService,
    private gameService: GameService,
    private addModalService: AddModalService
  ) {}

  ngOnInit(): void {
    this.userId = localStorage.getItem('userId') || '';

    if (this.userId) {
      console.log('Fetching books...');
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
          console.error("Unexpected Movies API response structure", response);
          return;
        }
        this.movies = response;
        console.log("🎬 Processed Movies:", this.movies);
        this.firstMovieImageUrl = this.movies.length ? this.movies[0].posterPath : null;
      },
      error: (err) => {
        console.error("Error fetching movies:", err);
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
          console.error(
            'Unexpected Albums API response structure',
            response
          );
          return;
        }

        this.albums = response;

        this.firstAlbumImageUrl = this.albums.length
          ? this.albums[0].artwork
          : null;
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
      },
      error: (err) => {
        console.error('Error fetching games:', err);
      },
    });
  }

  setActiveList(listType: string): void {
    this.showMoviesList = listType === 'movies';
    this.showBooksList = listType === 'books';
    this.showAlbumsList = listType === 'albums';
    this.showGamesList = listType === 'games';
  }

  getPosterUrl(posterPath: string): string {
    return posterPath
      ? `https://image.tmdb.org/t/p/w200${posterPath}`
      : 'https://via.placeholder.com/150';
  }
}
