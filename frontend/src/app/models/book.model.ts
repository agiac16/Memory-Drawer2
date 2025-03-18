export interface Book {
    apiId: string,
    title: string,
    authors: [string],
    rating: number,
    artwork: string,
    finished: boolean
    addedAt: Date;
  }