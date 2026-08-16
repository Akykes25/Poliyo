using NUnit.Framework;
using Poliyo.Presentation;
using Poliyo.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Poliyo.Core.EditModeTests
{
public sealed class PresentationUiBehaviorTests
{
    [TestCase(ElectoralMetric.Trust)]
    [TestCase(ElectoralMetric.VotingIntention)]
    [TestCase(ElectoralMetric.Rejection)]
    [TestCase(ElectoralMetric.Participation)]
    public void ElectoralFog_HidesEveryExactNationalMetric(ElectoralMetric metric)
    {
        string label = ElectoralMetricDisplay.FormatNational(metric, 9876.5m, estimatesHidden: true);

        Assert.That(label, Does.Not.Match(@"\d"), $"{metric} exposed an exact number during electoral fog.");
        Assert.That(label, Does.Contain(":"));
    }

    [TestCase(ElectoralMetric.Trust)]
    [TestCase(ElectoralMetric.VotingIntention)]
    [TestCase(ElectoralMetric.Rejection)]
    [TestCase(ElectoralMetric.Participation)]
    public void OpenInformation_ShowsEveryNationalMetric(ElectoralMetric metric)
    {
        string label = ElectoralMetricDisplay.FormatNational(metric, 42.5m, estimatesHidden: false);

        Assert.That(label, Does.Match(@"\d"));
    }

    [TestCase(30, false)]
    [TestCase(CampaignCalendar.FogStartDay, true)]
    [TestCase(CampaignCalendar.TotalCampaignDays, true)]
    public void ElectoralFog_UsesTheEntireConfiguredCalendarWindow(int currentDay, bool expected)
    {
        var calendar = new CampaignCalendar(currentDay);

        Assert.That(ElectoralMetricDisplay.ShouldHideEstimates(calendar), Is.EqualTo(expected));
    }

    [Test]
    public void CalendarInterviewDrawer_PreservesStateChangesMadeWhileControlsAreSuspended()
    {
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));

        try
        {
            Button consumedButton = CreateButton(canvasObject.transform, "ConsumedButton");
            Button newlyAvailableButton = CreateButton(canvasObject.transform, "NewlyAvailableButton");
            newlyAvailableButton.interactable = false;
            var drawerObject = new GameObject(
                "InterviewDrawer",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(CalendarInterviewDrawer));
            drawerObject.transform.SetParent(canvasObject.transform, false);
            Button confirmButton = CreateButton(drawerObject.transform, "ConfirmButton");
            CanvasGroup canvasGroup = drawerObject.GetComponent<CanvasGroup>();
            CalendarInterviewDrawer drawer = drawerObject.GetComponent<CalendarInterviewDrawer>();
            drawer.Configure(canvasGroup, confirmButton);

            drawer.Toggle();

            Assert.That(canvasGroup.alpha, Is.EqualTo(1f));
            Assert.That(canvasGroup.interactable, Is.True);
            Assert.That(canvasGroup.blocksRaycasts, Is.True);
            Assert.That(consumedButton.IsInteractable(), Is.False);
            Assert.That(newlyAvailableButton.IsInteractable(), Is.False);

            consumedButton.interactable = false;
            newlyAvailableButton.interactable = true;

            drawer.Close();

            Assert.That(canvasGroup.alpha, Is.Zero);
            Assert.That(canvasGroup.interactable, Is.False);
            Assert.That(canvasGroup.blocksRaycasts, Is.False);
            Assert.That(consumedButton.interactable, Is.False);
            Assert.That(consumedButton.IsInteractable(), Is.False);
            Assert.That(newlyAvailableButton.interactable, Is.True);
            Assert.That(newlyAvailableButton.IsInteractable(), Is.True);
            Assert.That(consumedButton.GetComponents<CanvasGroup>(), Is.Empty);
            Assert.That(newlyAvailableButton.GetComponents<CanvasGroup>(), Is.Empty);
        }
        finally
        {
            Object.DestroyImmediate(canvasObject);
        }
    }

    private static Button CreateButton(Transform parent, string name)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        return buttonObject.GetComponent<Button>();
    }
}
}
