import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { Movie } from '../../models/movie.model';
import { Book } from '../../models/book.model';
import { Album } from '../../models/album.model';
import { Link } from '../../models/link.model';
import { Game } from '../../models/game.model';

import { BookService } from '../../services/books/book.service';
import { AlbumsService } from '../../services/albums/albums.service';
import { LinkService } from '../../services/links/link.service';
import { GameService } from '../../services/games/game.service';
import { MovieService } from '../../services/movies/movie.service';
import { AddModalService } from '../../services/addItemModal/add-modal.service';


@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})

export class DashboardComponent implements OnInit {
  isLoggedIn: boolean = true;
  // movies
  movies: Movie[] = [];
  albums: Album[] = [];
  books: Book[] = [];
  links: Link[] = [];
  games: Game[] = [];
  userId: string = '';
  firstBookImageUrl: string | null = null;
  firstMovieImageUrl: string | null = null;
  firstAlbumImageUrl: string | null = null;
  firstGameImageUrl: string | null = null;
  showMoviesList: boolean = false;
  showBooksList: boolean = false;
  showAlbumsList: boolean = false;
  showGamesList: boolean = false;
  showLinksList: boolean = false;

  constructor(
    private movieService: MovieService,
    private albumService: AlbumsService,
    private bookService: BookService,
    private linkService: LinkService,
    private gameService: GameService,
    private addModalService: AddModalService,
    private http: HttpClient,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.userId = localStorage.getItem('userId') || '';

    if (this.userId) {
      this.loadMovies();
      this.loadAlbums();
      this.loadBooks();
      this.loadLinks();
      this.loadGames();
      this.loadFirstGameImage();
      this.loadFirstBookImage();
      this.loadFirstMovieImage();
      this.loadFirstAlbumImage();
    } else {
      console.error('User ID not found.');
    }
}

  openModal(): void {
    this.addModalService.open();
  }

  loadFirstBookImage(): void {
    this.bookService.getFirstBookImageUrl(this.userId).subscribe((imageUrl) => {
      this.firstBookImageUrl = imageUrl;
    });
  }

  loadFirstMovieImage(): void {
   this.movieService.getFirstMovieImageUrl(this.userId).subscribe((imageUrl) => {
    this.firstMovieImageUrl = imageUrl;
   });
  }

  loadFirstAlbumImage(): void {
    this.albumService
      .getFirstAlbumImageUrl(this.userId)
      .subscribe((imageUrl) => {
        console.log('First album image URL:', imageUrl); // Debugging log
        this.firstAlbumImageUrl = imageUrl;
      });
  }

  loadFirstGameImage(): void {
    this.gameService
      .getFirstGameImageUrl(this.userId)
      .subscribe((imageUrl) => {
        console.log('First game image URL:', imageUrl); // Debugging log
        this.firstGameImageUrl = imageUrl;
      });
  }

  loadBooks(): void {
    this.bookService.getUserBooks(this.userId).subscribe({
      next: (response) => {
        this.books = response.data;
      },
      error: (err) => {
        console.error('Error fetching books:', err);
      },
    });
  }

  loadAlbums(): void {
    this.albumService.getUserAlbums(this.userId).subscribe({
      next: (response) => {
        this.albums = response.data;
      },
      error: (err) => {
        console.error('Error fetching albums:', err);
      },
    });
  }

  loadMovies(): void {
    this.movieService.getUserMovies(this.userId).subscribe({
      next: (response) => {
        this.movies = response?.data ?? [];
      },
      error: (err) => {
        console.error('Error fetching movies:', err);
      },
    });
  }

  loadGames(): void {
    this.gameService.getUserGames(this.userId).subscribe({
      next: (response) => {
        this.games = response.data;
      },
      error: (err) => {
        console.error('Error fetching movies:', err);
      },
    });
  }


  loadLinks(): void {
    this.linkService.getUserLinks(this.userId).subscribe({
      next: (response) => {
        this.links = response.data;
      },
      error: (err) => {
        console.error('Error fetching links:', err);
      },
    });
  }

  setActiveList(listType: string): void {
    this.showMoviesList = listType === 'movies';
    this.showAlbumsList = listType === 'albums';
    this.showBooksList = listType === 'books';
    this.showLinksList = listType === 'links';
    this.showGamesList = listType === 'games';
  }

  checkLoginStatus() {
    this.isLoggedIn = !!localStorage.getItem('token');
  }

  // get movie poster
  getPosterUrl(posterPath: string): string {
    return this.movieService.getPosterUrl(posterPath);
  }

  logout() {
    localStorage.removeItem('token');
    this.checkLoginStatus();
    this.router.navigate(['']);

    setTimeout(() => {
      this.checkLoginStatus();
    }, 0);
  }
}
