import { AfterViewInit, Directive, inject } from '@angular/core';
import { AccordionComponent } from '@laczynski/ui';

@Directive({
  selector: 'ui-accordion[appDefaultExpanded]'
})
export class DefaultExpandedAccordionDirective implements AfterViewInit {
  private readonly accordion = inject(AccordionComponent);

  ngAfterViewInit(): void {
    this.accordion.expanded.set(true);
  }
}
