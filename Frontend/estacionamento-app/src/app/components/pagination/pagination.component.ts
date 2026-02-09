import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.css']
})
export class PaginationComponent {
  @Input() currentPage: number = 1;
  @Input() totalPages: number = 1;
  @Input() pageSize: number = 10;
  @Input() totalCount: number = 0;
  @Input() hasPreviousPage: boolean = false;
  @Input() hasNextPage: boolean = false;

  @Output() pageChanged = new EventEmitter<number>();
  @Output() pageSizeChanged = new EventEmitter<number>();

  pageSizes = [5, 10, 20, 50, 100];

  // Propriedade local para o ngModel
  selectedPageSize: number = 10;

  ngOnInit(): void {
    this.selectedPageSize = this.pageSize;
  }

  ngOnChanges(): void {
    this.selectedPageSize = this.pageSize;
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.pageChanged.emit(page);
    }
  }

  onPageSizeChange(): void {
    console.log('Page size mudou para:', this.selectedPageSize);
    this.pageSizeChanged.emit(this.selectedPageSize);
  }

  getPages(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 7;
    
    if (this.totalPages <= maxPagesToShow) {
      // Se tiver poucas páginas, mostra todas
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Lógica para mostrar páginas com "..."
      const leftSide = Math.max(1, this.currentPage - 2);
      const rightSide = Math.min(this.totalPages, this.currentPage + 2);

      if (leftSide > 2) {
        pages.push(1);
        if (leftSide > 3) {
          pages.push(-1); // -1 representa "..."
        }
      } else {
        for (let i = 1; i < leftSide; i++) {
          pages.push(i);
        }
      }

      for (let i = leftSide; i <= rightSide; i++) {
        pages.push(i);
      }

      if (rightSide < this.totalPages - 1) {
        if (rightSide < this.totalPages - 2) {
          pages.push(-1); // -1 representa "..."
        }
        pages.push(this.totalPages);
      } else {
        for (let i = rightSide + 1; i <= this.totalPages; i++) {
          pages.push(i);
        }
      }
    }

    return pages;
  }

  getStartItem(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getEndItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }
}
