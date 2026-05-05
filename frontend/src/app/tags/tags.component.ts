import { Component, OnInit } from '@angular/core';
import { TableModule } from 'primeng/table';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

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
  tags: Tag[] = [];
  loading = false;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadTags();
  }

  loadTags() {
    this.loading = true;
    this.http.get<{ data: Tag[], totalRecords: number }>('http://localhost:8080/api/tags')
      .subscribe({
        next: (response) => {
          this.tags = response.data;
          this.loading = false;
        },
        error: (err) => {
          console.error(err);
          this.loading = false;
        }
      });
  }
}