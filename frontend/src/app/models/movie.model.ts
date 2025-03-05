export interface Movie {
    _id: string;
    title: string;
    apId: string;
    rating?: number | null; 
    posterPath: string;
  }