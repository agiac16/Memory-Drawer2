export interface Movie {
    _id: string;
    title: string;
    apiId: string;
    rating?: number | null; 
    posterPath: string;
    addedAt: Date;
  }