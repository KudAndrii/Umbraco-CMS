  import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockGridEditorName = 'TestBlockGridEditor';
const elementTypeName = 'BlockGridElement';
const dataTypeName = 'Textstring';
const groupName = 'testGroup';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockGridEditorName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockGridEditorName);
});

// Settings

test('can add a label to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const labelText = 'TestLabel';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await page.pause();
  await umbracoUi.dataType.enterBlockLabelText(labelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName,elementTypeId, labelText)).toBeTruthy();
});

test('can remove a label from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const labelText = 'TestLabel';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithLabel(blockGridEditorName, elementTypeId, labelText);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName,elementTypeId, labelText)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockLabelText();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName,elementTypeId, labelText)).toBeFalsy();
});

test('can open content model in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.openBlockContentModel();

  // Assert
  await umbracoUi.dataType.isElementWorkspaceOpenInBlock(elementTypeName);
});

test('can add a settings model to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.addBlockSettingsModel(secondElementName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeTruthy();
});

test('can remove a settings model from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithContentAndSettingsElementType(blockGridEditorName, contentElementTypeId, settingsElementTypeId);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockSettingsModel();
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeFalsy();
});

  test('can enable allow in root from a block', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
    const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
    await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

    // Act
    await umbracoUi.dataType.goToDataType(blockGridEditorName);
    await umbracoUi.dataType.goToBlockWithName(elementTypeName);
    await umbracoUi.dataType.clickAllowInRootForBlock();
    await umbracoUi.dataType.clickSubmitButton();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesBlockHaveAllowInRootEnabled(blockGridEditorName, contentElementTypeId)).toBeTruthy();
  });

  test('can enable allow in areas from a block', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
    const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
    await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

    // Act
    await umbracoUi.dataType.goToDataType(blockGridEditorName);
    await umbracoUi.dataType.goToBlockWithName(elementTypeName);
    await umbracoUi.dataType.clickAllowInAreasForBlock();
    await umbracoUi.dataType.clickSubmitButton();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesBlockHaveAllowInAreasEnabled(blockGridEditorName, contentElementTypeId)).toBeTruthy();
  });

  test('can add available column spans to a block', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
    const columnSpan = [1];
    const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
    await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

    // Act
    await umbracoUi.dataType.goToDataType(blockGridEditorName);
    await umbracoUi.dataType.goToBlockWithName(elementTypeName);
    await umbracoUi.dataType.clickShowResizeOptions();
    await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
    await umbracoUi.dataType.clickSubmitButton();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, columnSpan)).toBeTruthy();
  });

  test('can add multiple available column spans to a block', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
    const columnSpan = [1,3,6,8];
    const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
    await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

    // Act
    await umbracoUi.dataType.goToDataType(blockGridEditorName);
    await umbracoUi.dataType.goToBlockWithName(elementTypeName);
    await umbracoUi.dataType.clickShowResizeOptions();
    await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
    await umbracoUi.dataType.clickSubmitButton();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, columnSpan)).toBeTruthy();
  });

  test('can remove an available column span from a block', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
    const columnSpan = [4];
    const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
    await umbracoApi.dataType.createBlockGridDataTypeWithSizeOptions(blockGridEditorName, contentElementTypeId, columnSpan[0]);

    // Act
    await umbracoUi.dataType.goToDataType(blockGridEditorName);
    await umbracoUi.dataType.goToBlockWithName(elementTypeName);
    await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
    await umbracoUi.dataType.clickSubmitButton();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, [])).toBeTruthy();
  });


  test('can add an available row span to a block', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can add multiple available row spans to a block', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can remove available row spans from a block', async ({page, umbracoApi, umbracoUi}) => {

  });
