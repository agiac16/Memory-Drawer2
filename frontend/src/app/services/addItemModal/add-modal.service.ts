import { 
  Injectable, 
  ComponentRef, 
  EnvironmentInjector, 
  createComponent, 
  ApplicationRef 
} from '@angular/core';
import { AddModalComponent } from '../../components/add-modal/add-modal.component';

@Injectable({
  providedIn: 'root'
})
export class AddModalService {
  newModalComponent!: ComponentRef<AddModalComponent>;

  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector,
  ) {}

  open(): void {
    this.newModalComponent = createComponent(AddModalComponent, {
      environmentInjector: this.injector
    });

    document.body.appendChild(this.newModalComponent.location.nativeElement);
    this.appRef.attachView(this.newModalComponent.hostView);  
  }

  close(): void { 
    if (this.newModalComponent) { 
      this.appRef.detachView(this.newModalComponent.hostView);
      this.newModalComponent.destroy();
    }
  }
}

