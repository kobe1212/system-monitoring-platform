import { Component, OnInit } from '@angular/core';
import { MetricService } from '../../core/services/metric.service';
import { CreateMetric, SystemMetric, MetricQuery } from '../../shared/models/system-metric.model';

@Component({
  selector: 'app-metrics',
  templateUrl: './metrics.component.html',
  styleUrls: ['./metrics.component.css']
})
export class MetricsComponent implements OnInit {
  metrics: SystemMetric[] = [];
  isLoading = false;
  errorMessage = '';
  showCreateForm = false;

  newMetric: CreateMetric = {
    metricName: '',
    value: 0,
    unit: '',
    source: '',
    tags: ''
  };

  query: MetricQuery = {
    pageNumber: 1,
    pageSize: 50
  };

  constructor(private metricService: MetricService) {}

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.metricService.getMetrics(this.query).subscribe({
      next: (data) => {
        this.metrics = data;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load metrics';
        console.error('Error loading metrics:', error);
        this.isLoading = false;
      }
    });
  }

  createMetric(): void {
    if (!this.validateMetric()) {
      return;
    }

    this.metricService.createMetric(this.newMetric).subscribe({
      next: (metric) => {
        this.metrics.unshift(metric);
        this.resetForm();
        this.showCreateForm = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to create metric';
        console.error('Error creating metric:', error);
      }
    });
  }

  validateMetric(): boolean {
    if (!this.newMetric.metricName || !this.newMetric.unit || !this.newMetric.source) {
      this.errorMessage = 'Please fill in all required fields';
      return false;
    }
    return true;
  }

  resetForm(): void {
    this.newMetric = {
      metricName: '',
      value: 0,
      unit: '',
      source: '',
      tags: ''
    };
    this.errorMessage = '';
  }

  toggleCreateForm(): void {
    this.showCreateForm = !this.showCreateForm;
    if (!this.showCreateForm) {
      this.resetForm();
    }
  }

  applyFilter(): void {
    this.loadMetrics();
  }

  clearFilter(): void {
    this.query = {
      pageNumber: 1,
      pageSize: 50
    };
    this.loadMetrics();
  }
}
