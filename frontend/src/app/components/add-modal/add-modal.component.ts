import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AddModalService } from '../../services/addItemModal/add-modal.service';
import { MovieService } from '../../services/movies/movie.service';
import { BookService } from '../../services/books/book.service';
import { AlbumsService } from '../../services/albums/albums.service';
import { GameService } from '../../services/games/game.service';

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

  // activeCategory: any = null;
  // categories = [
  //   { type: 'books', label: 'Books to Read', items: this.books, firstItem: null },
  //   { type: 'movies', label: 'Movies & Shows', items: this.movies, firstItem: null },
  //   { type: 'albums', label: 'Albums to Listen', items: this.albums, firstItem: null },
  //   { type: 'games', label: 'Games to Play', items: this.games, firstItem: null },
  //   { type: 'links', label: 'Saved Links', items: this.links, firstItem: null },
  // ];

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
  ) { }

  ngOnInit() {
    // this.updateFirstItems();
  }

  // updateFirstItems() {
  //   this.categories.forEach((category) => {
  //     category.firstItem = category.items.length ? category.items[0] : null;
  //   });
  // }

  // setActiveList(type: string) {
  //   this.activeCategory = this.categories.find((c) => c.type === type);
  // }


  getAlbumImage(album: any): string {
    if (!album.image || album.image.length === 0) {
      return 'https://via.placeholder.com/150';
    }
  
    // ✅ Get the largest available image (Extralarge > Large > Medium > Small)
    const preferredSizes = ['extralarge', 'large', 'medium', 'small'];
    const foundImage = album.image.find((img: any) =>
      preferredSizes.includes(img.size.toLowerCase())
    );
  
    return foundImage?.['#text'] ?? 'https://via.placeholder.com/150';
  }

  onSearch() { 
    if (!this.searchQuery.trim()) return; // leading spaces
    
    // search api based on selected type
    switch (this.selectedType) {
      case 'movie':
          this.movieService.searchMovie(this.searchQuery).subscribe({
              next: (results) => {
                  console.log('Movie API Response:', results);
                  this.searchResults = results;
              },
              error: (err) => {
                  console.error('Movie search failed:', err);
                  this.searchResults = [];
              }
          });
          break;

      case 'book':
          this.bookService.searchBook(this.searchQuery).subscribe({
              next: (results) => {
                  console.log('Book API Response:', results);
                  this.searchResults = results;
              },
              error: (err) => {
                  console.error('Book search failed:', err);
                  this.searchResults = [];
              }
          });
          break;

      case 'album':
          this.albumService.searchAlbums(this.searchQuery).subscribe({
              next: (results) => {
                  console.log('Album API Response:', results);
                  this.searchResults = results;
              },
              error: (err) => {
                  console.error('Album search failed:', err);
                  this.searchResults = [];
              }
          });
          break;

      case 'game':
          this.gameService.searchGames(this.searchQuery).subscribe({
              next: (results) => {
                  console.log('Game API Response:', results);
                  this.searchResults = results;
              },
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

  close() {
    this.modalService.close()
  }

  onTypeSelect(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.selectedType = target.value;
  }

  
}
