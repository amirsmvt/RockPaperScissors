using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

    [SerializeField] private List<View> views = new List<View>();
    [SerializeField] private View viewsDefault;

    [SerializeField] private GameObject _game;

    private View _currentView;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        foreach (View view in views)
        {
            view.Initialize();
            view.Hide();
        }
        if (viewsDefault != null)
            Show(viewsDefault);
    }

    public void Show<TView>(object args = null) where TView : View
    {
        foreach (View view in views)
        {
            if (view is not TView) continue;
            if (_currentView != null)
                _currentView.Hide();

            view.Show(args);
            _currentView = view;
            break;
        }
    }

    public void Show(View view, object args = null)
    {
        if (_currentView != null)
            _currentView.Hide();

        view.Show(args);
        _currentView = view;
    }

    public void ShowGame()
    {
        if (_game != null)
            _game.SetActive(true);

        if (viewsDefault != null)
            viewsDefault.Hide();
    }
}
