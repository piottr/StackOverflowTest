import { Component, OnInit } from '@angular/core';
import { TableModule } from 'primeng/table';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { TagsService, TagPercentageDto, PagedResult, PrimeNgRequestDto } from '../services/tags.service';

interface Tag {
  name: string;
  count: number;
  percentage: number;
}

@Component({
  selector: 'app-tags',
  standalone: true,
  imports: [TableModule, CommonModule],
  templateUrl: './tags.component.html',
  styleUrl: './tags.component.scss'
})
export class TagsComponent implements OnInit {
  tags: TagPercentageDto[] = [];
  loading = false;
  totalRecords = 0;

  constructor(private tagsService: TagsService) {}

  ngOnInit() {
    // Lazy loading will handle initial load
  }

  loadTags(event: any) {
    this.loading = true;
    const request: PrimeNgRequestDto = {
      first: event.first,
      rows: event.rows,
      sortField: event.sortField,
      sortOrder: event.sortOrder
    };

    this.tagsService.getTags(request).subscribe({
      next: (response: PagedResult<TagPercentageDto>) => {
        this.tags = response.data;
        this.totalRecords = response.totalRecords;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }
}