using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartWarehouse.Core;

namespace SmartWarehouse.Tests;

[TestClass]
public class WarehouseSensorTests
{
    [TestMethod]
    public void Constructor_ShouldSetDefaultValues()
    {
        string sensorId = "SENSOR-001";
        string locationTag = "Cold-Vault-01";

        WarehouseSensor sensor =
            new WarehouseSensor(sensorId, locationTag);

        Assert.AreEqual(sensorId, sensor.SensorId);
        Assert.AreEqual(locationTag, sensor.LocationTag);
        Assert.AreEqual(0.0, sensor.CurrentTemperature);
        Assert.IsFalse(sensor.IsActive);
        Assert.IsFalse(sensor.IsAlertTriggered);
        Assert.AreEqual(4.0, sensor.CriticalThresholdCelsius);
    }

    [TestMethod]
    public void Constructor_ShouldAcceptCustomThreshold()
    {
        double threshold = 10.0;

        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01",
                threshold);

        Assert.AreEqual(
            threshold,
            sensor.CriticalThresholdCelsius);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void Constructor_ShouldRejectInvalidSensorId(
        string? sensorId)
    {
        string locationTag = "Cold-Vault-01";

        Action action = () =>
            new WarehouseSensor(
                sensorId!,
                locationTag);

        Assert.Throws<ArgumentException>(action);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void Constructor_ShouldRejectInvalidLocationTag(
        string? locationTag)
    {
        string sensorId = "SENSOR-001";

        Action action = () =>
            new WarehouseSensor(
                sensorId,
                locationTag!);

        Assert.Throws<ArgumentException>(action);
    }

    [TestMethod]
    public void Activate_ShouldMakeSensorActive()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();

        Assert.IsTrue(sensor.IsActive);
    }

    [TestMethod]
    public void Activate_ShouldBeIdempotent()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();
        sensor.Activate();

        Assert.IsTrue(sensor.IsActive);
    }

    [TestMethod]
    public void Deactivate_ShouldMakeSensorInactive()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();

        sensor.Deactivate();

        Assert.IsFalse(sensor.IsActive);
    }

    [TestMethod]
    public void Deactivate_ShouldClearAlert()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(10.0);

        sensor.Deactivate();

        Assert.IsFalse(sensor.IsActive);
        Assert.IsFalse(sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void RecordReading_ShouldRejectInactiveSensor()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        Action action = () =>
            sensor.RecordReading(5.0);

        Assert.Throws<InvalidOperationException>(action);
    }

    [DataTestMethod]
    [DataRow(3.9, false)]
    [DataRow(4.0, true)]
    [DataRow(4.1, true)]
    public void RecordReading_ShouldSetCorrectAlert(
        double temperature,
        bool expectedAlert)
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();

        sensor.RecordReading(temperature);

        Assert.AreEqual(
            temperature,
            sensor.CurrentTemperature);

        Assert.AreEqual(
            expectedAlert,
            sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void RecordReading_ShouldAcceptMinus50()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();

        sensor.RecordReading(-50.0);

        Assert.AreEqual(
            -50.0,
            sensor.CurrentTemperature);
    }

    [TestMethod]
    public void RecordReading_ShouldAccept80()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();

        sensor.RecordReading(80.0);

        Assert.AreEqual(
            80.0,
            sensor.CurrentTemperature);
    }

    [DataTestMethod]
    [DataRow(-50.1)]
    [DataRow(80.1)]
    public void RecordReading_ShouldRejectInvalidTemperature(
        double temperature)
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();

        Action action = () =>
            sensor.RecordReading(temperature);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [TestMethod]
    public void UpdateThreshold_ShouldChangeThreshold()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        double newThreshold = 10.0;

        sensor.UpdateThreshold(newThreshold);

        Assert.AreEqual(
            newThreshold,
            sensor.CriticalThresholdCelsius);
    }

    [DataTestMethod]
    [DataRow(-30.0)]
    [DataRow(50.0)]
    public void UpdateThreshold_ShouldAcceptBoundaryValues(
        double threshold)
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.UpdateThreshold(threshold);

        Assert.AreEqual(
            threshold,
            sensor.CriticalThresholdCelsius);
    }

    [DataTestMethod]
    [DataRow(-30.1)]
    [DataRow(50.1)]
    public void UpdateThreshold_ShouldRejectInvalidValues(
        double threshold)
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        Action action = () =>
            sensor.UpdateThreshold(threshold);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [TestMethod]
    public void UpdateThreshold_ShouldTurnAlertOnWhenNewThresholdIsLower()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(5.0);

        sensor.UpdateThreshold(4.0);

        Assert.IsTrue(sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void UpdateThreshold_ShouldTurnAlertOffWhenNewThresholdIsHigher()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(5.0);

        sensor.UpdateThreshold(6.0);

        Assert.IsFalse(sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void UpdateThreshold_ShouldTriggerAlertAtExactTemperature()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(5.0);

        sensor.UpdateThreshold(5.0);

        Assert.IsTrue(sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void RecordReading_ShouldClearAlertWhenTemperatureDrops()
    {
        WarehouseSensor sensor =
            new WarehouseSensor(
                "SENSOR-001",
                "Cold-Vault-01");

        sensor.Activate();
        sensor.RecordReading(10.0);

        sensor.RecordReading(3.0);

        Assert.IsFalse(sensor.IsAlertTriggered);
    }
}