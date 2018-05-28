﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using TextureViewer.Annotations;
using TextureViewer.Commands;
using TextureViewer.Models;
using TextureViewer.Models.Filter;
using TextureViewer.ViewModels.Filter;
using TextureViewer.Views;

namespace TextureViewer.ViewModels
{
    public class FiltersViewModel : INotifyPropertyChanged, IDropTarget
    {
        private readonly Models.Models models;

        private class FilterItem
        {
            public FilterModel Model { get; }
            public FilterListBoxItem ListView { get; }
            public FilterParametersViewModel Parameters { get; }

            public FilterItem(FiltersViewModel parent, FilterModel model)
            {
                Model = model;
                ListView = new FilterListBoxItem(parent, model);
                Parameters = new FilterParametersViewModel(model);
            }

            public void Dispose(OpenGlContext context)
            {
                var disable = context.Enable();
                Model.Dispose();
                if(disable) context.Disable();
            }
        }

        private List<FilterItem> items = new List<FilterItem>();

        public FiltersViewModel(Models.Models models)
        {
            this.models = models;
            this.ApplyCommand = new ApplyFiltersCommand(this);
            this.CancelCommand = new CancelFiltersCommand(this);
        }

        private List<FilterListBoxItem> availableFilter = new List<FilterListBoxItem>();
        public List<FilterListBoxItem> AvailableFilter
        {
            get => availableFilter;
            set
            {
                availableFilter = value;
                OnPropertyChanged(nameof(AvailableFilter));
            }
        }

        private void UpdateAvailableFilter()
        {
            var res = new List<FilterListBoxItem>();
            foreach (var filterItem in items)
            {
                res.Add(filterItem.ListView);
            }

            AvailableFilter = res;
        }

        private FilterListBoxItem selectedFilter = null;
        public FilterListBoxItem SelectedFilter
        {
            get => selectedFilter;
            set
            {
                if (Equals(selectedFilter, value)) return;
                selectedFilter = value;
                OnPropertyChanged(nameof(SelectedFilter));
                OnPropertyChanged(nameof(SelectedFilterProperties));
            }
        }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        public ObservableCollection<object> SelectedFilterProperties
        {
            get
            {
                foreach (var filterItem in items)
                {
                    if (ReferenceEquals(filterItem.ListView, selectedFilter))
                        return filterItem.Parameters.View;
                }

                return null;
            }
        }

        private bool hasChanges = false;
        public bool HasChanges
        {
            get => hasChanges;
            set
            {
                if (value == hasChanges) return;
                hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public void AddFilter(FilterModel filter)
        {
            var item = new FilterItem(this, filter);
            items.Add(item);            
            UpdateAvailableFilter();

            // select the added element
            SelectedFilter = item.ListView;
            UpdateHasChanges();

            // register on changed for apply and cancel button
            item.Parameters.Changed += (sender, args) => UpdateHasChanges();
        }

        public void RemoveFilter(FilterModel filter)
        {
            var removeItem = items.Find(item => item.Model.Equals(filter));
            items.Remove(removeItem);
            UpdateAvailableFilter();
            UpdateHasChanges();

            // dispose of shader data
            if(!models.Filter.IsUsed(removeItem.Model))
                removeItem.Dispose(models.GlContext);
        }

        /// <summary>
        /// applies all pending changes from the parameters
        /// </summary>
        public void Apply()
        {
            // apply the current parameters
            foreach (var filterItem in items)
            {
                filterItem.Parameters.Apply();
            }

            // exchange model lists
            var newModels = new List<FilterModel>();
            foreach (var filterItem in items)
            {
                newModels.Add(filterItem.Model);
            }

            models.Filter.Apply(newModels, models.GlContext);

            UpdateHasChanges();
        }

        /// <summary>
        /// reverts all changes since the last apply
        /// </summary>
        public void Cancel()
        {
            // restore old state
            var filters = new List<FilterModel>();
            foreach (var filterItem in items)
            {
                filters.Add(filterItem.Model);
            }
            
            // dispose all filter which were never used after import
            FiltersModel.DisposeUnusedFilter(models.Filter.Filter, filters, models.GlContext);

            // restore list
            var newItems = new List<FilterItem>();
            foreach (var filterModel in models.Filter.Filter)
            {
                // find the correspinging FilterItem if it still exists
                var filterItem = items.Find(fi => ReferenceEquals(fi.Model, filterModel));
                if (filterItem != null)
                {
                    newItems.Add(filterItem);
                    filterItem.Parameters.Cancel();
                }
                else
                {
                    // create a new filter item
                    var item = new FilterItem(this, filterModel);
                    newItems.Add(item);
                    // register on changed for apply and cancel button
                    item.Parameters.Changed += (sender, args) => UpdateHasChanges();
                }
            }

            items = newItems;

            UpdateAvailableFilter();
            UpdateHasChanges();
        }

        /// <summary>
        /// compares the view model with the model data to determine if anything changed.
        /// Sets HasChanges
        /// </summary>
        private void UpdateHasChanges()
        {
            HasChanges = CalculateHasChanges();
        }

        /// <summary>
        /// compares the view model with the model data to determine if anything changed
        /// </summary>
        private bool CalculateHasChanges()
        {
            if (models.Filter.NumFilter != items.Count) return true;

            // same amount of filter. do they match?
            for (int i = 0; i < items.Count; i++)
            {
                if (!ReferenceEquals(models.Filter.Filter[i], items[i].Model)) return true;
            }

            // have the parameters changed?
            foreach (var filterItem in items)
            {
                if (filterItem.Parameters.HasChanged) return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DragOver(IDropInfo dropInfo)
        {
            // enable drop if both items are filter list box items
            if (dropInfo.Data is FilterListBoxItem && dropInfo.TargetItem is FilterListBoxItem)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var source = dropInfo.Data as FilterListBoxItem;
            var dest = dropInfo.TargetItem as FilterListBoxItem;

            // swap both items
            var idx1 = AvailableFilter.FindIndex(i => ReferenceEquals(i, source));
            var idx2 = AvailableFilter.FindIndex(i => ReferenceEquals(i, dest));
            if (idx1 < 0 || idx2 < 0) return;

            {
                var tmp = items[idx1];
                items[idx1] = items[idx2];
                items[idx2] = tmp;
            }

            UpdateAvailableFilter();
            UpdateHasChanges();
        }

        public bool HasKeyToInvoke(Key key)
        {
            return items.Any(f => f.Parameters.HasKeyToInvoke(key));
        }

        public void InvokeKey(Key key)
        {
            foreach (var filterItem in items)
            {
                filterItem.Parameters.InvokeKey(key);
            }
        }
    }
}