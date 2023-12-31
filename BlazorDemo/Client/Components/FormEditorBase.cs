﻿using BlazorDemo.Client.Utility;
using BlazorDemo.Shared.Utility;
using BlazorDemo.Shared.Validation;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Linq.Expressions;

namespace BlazorDemo.Client.Components;

public abstract class FormEditorBase<T> : ValidatableComponent
{
    private readonly bool _isStringValue = typeof(T).Is<string>();
    private MemberValidator _memberValidator = MemberNonValidator.Instance;

    protected ElementReference _editorElement;
    protected T? _value;

    [Parameter, EditorRequired]
    public T? Value { get; set; }

    [Parameter, EditorBrowsable(EditorBrowsableState.Never)]
    public Expression<Func<T?>> ValueExpression { get; set; } = null!;

    [Parameter]
    public EventCallback<T?> ValueChanged { get; set; }

    [Parameter]
    public IEqualityComparer<T> EqualityComparer { get; set; } = EqualityComparer<T>.Default;

    [Parameter]
    public TypeConverter? TypeConverter { get; set; }

    [Parameter]
    public Func<object?, Task<string?>>? CustomValidator { get; set; }

    protected string? _label;

    [Parameter]
    public string? Label { get; set; }

    protected string? _note;

    [Parameter]
    public string? Note { get; set; }

    protected string? _title;

    [Parameter]
    public string? Title { get; set; }

    protected string? _placeholder;

    [Parameter]
    public string? Placeholder { get; set; }

    private bool _hasRequiredAttribute;
    protected bool _isRequired => _hasRequiredAttribute || IsRequired;

    [Parameter]
    public bool IsRequired { get; set; }

    protected string? _id;

    [Parameter]
    public string? Id { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        TrySetParameterField(parameters, nameof(Label), ref _label);
        TrySetParameterField(parameters, nameof(Note), ref _note);
        TrySetParameterField(parameters, nameof(Title), ref _title);
        TrySetParameterField(parameters, nameof(Placeholder), ref _placeholder);
        TrySetParameterField(parameters, nameof(Id), ref _id);

        return base.SetParametersAsync(parameters);
    }

    private static void TrySetParameterField<TField>(ParameterView parameters, string parameterName, ref TField? field)
    {
        if (parameters.TryGetValue(parameterName, out TField? value)) {
            field = value;
        }
    }

    protected override void OnParametersSet() => _value = Value;

    protected override void OnInitialized()
    {
        var provider = MemberMetadataProvider.Create(ValueExpression);

        _hasRequiredAttribute = provider.HasRequiredAttribute;

        _label ??= provider.Label;
        _note ??= provider.Note;
        _title ??= provider.Title;
        _placeholder ??= provider.Placeholder;

        _id = Id ?? "_" + Guid.NewGuid();

        _memberValidator = new MemberValidatorBuilder()
            .WithAttributeValidation(provider, _label)
            .WithCustomValidation(CustomValidator)
            .Build();

        base.OnInitialized();
    }

    protected Task SetValueAsync(T? value)
    {
        if (EqualityComparer.Equals(_value, value)) {
            return Task.CompletedTask;
        }

        _value = value;
        return ValueChanged.InvokeAsync(value);
    }

    protected Task EditorChangedAsync(ChangeEventArgs e)
    {
        if (TryGetValue(e.Value, out T? result)) {
            return SetValueAsync(result);
        }

        return ResetToCurrentValueAsync();
    }

    protected async Task ResetToCurrentValueAsync()
    {
        T? value = _value;

        await SetValueAsync(default);
        await Task.Delay(1);
        await SetValueAsync(value);
    }

    protected override Task<string?> GetValidationErrorAsync() => _memberValidator.ValidateAsync(_value);

    protected ValueTask FocusAsync() => _editorElement.FocusAsync();

    private bool TryGetValue(object? value, out T? result)
    {
        try {
            result = value switch
            {
                null => default,
                string s when !_isStringValue && string.IsNullOrWhiteSpace(s) => default,
                _ when TypeConverter is not null => (T?)TypeConverter.ConvertFrom(value),
                _ => EditorHelpers.ConvertFrom<T>(value)
            };

            return true;
        }
        catch (Exception) {
            result = default;
            return false;
        }
    }
}
