import React, { useEffect, useState } from 'react';
import { matchingHints, matchingOptions, SettingsDialogProps, suggestedModel } from './model';
import { AppInfo, MatchingMode, OllamaStatus } from '../../model';
import { useProfileContext } from '../../profile-context';
import * as api from '../../api';
import Modal from '../../../../components/modal';
import Button from '../../../../components/button';
import SegmentedControl from '../../../../components/segmented-control';
import TextField from '../../../../components/text-field';
import Select from '../../../../components/select';

import './styles.scss';

const SettingsDialog: React.FC<SettingsDialogProps> = ({ onClose }) => {
    const { settings, saveSettings } = useProfileContext();
    const [mode, setMode] = useState<MatchingMode>(settings?.matchingMode ?? 'keyword');
    const [url, setUrl] = useState(settings?.ollamaUrl ?? '');
    const [model, setModel] = useState(settings?.ollamaModel ?? '');
    const [status, setStatus] = useState<OllamaStatus | null>(null);
    const [checking, setChecking] = useState(false);
    const [about, setAbout] = useState<AppInfo | null>(null);

    const check = async () => {
        setChecking(true);
        try {
            const next = await api.getOllamaStatus(url.trim());
            setStatus(next);
            if (next.reachable && next.models.length > 0 && !next.models.includes(model)) setModel(next.models[0]);
        } catch {
            setStatus({ reachable: false, models: [], error: 'Could not check Ollama.' });
        } finally {
            setChecking(false);
        }
    };

    useEffect(() => {
        check();
        api.getAbout().then(setAbout).catch(() => setAbout(null));
    }, []);

    const save = async () => {
        if (!settings) return;
        await saveSettings({ ...settings, matchingMode: mode, ollamaUrl: url.trim(), ollamaModel: model });
        onClose();
    };

    const models = status?.reachable ? status.models : [];

    return (
        <Modal
            title="Settings"
            onClose={onClose}
            actions={(
                <>
                    <Button variant="secondary" onClick={onClose}>Cancel</Button>
                    <Button onClick={save} disabled={!settings || (mode === 'ollama' && !url.trim())}>Save</Button>
                </>
            )}
        >
            <div className="settings-dialog">
                <div className="settings-dialog-group">
                    <span className="settings-dialog-label">Matching</span>
                    <SegmentedControl label="Matching" value={mode} options={matchingOptions} onChange={setMode} />
                    <p className="settings-dialog-hint">{matchingHints[mode]}</p>
                </div>

                {mode === 'ollama' && (
                    <div className="settings-dialog-group">
                        <div className="settings-dialog-address">
                            <TextField className="settings-dialog-url" label="Ollama address" value={url} onChange={setUrl} spellCheck={false} />
                            <Button variant="secondary" onClick={check} disabled={checking || !url.trim()}>Check</Button>
                        </div>

                        <p className={`settings-dialog-status${status?.reachable ? ' settings-dialog-status--ok' : ''}`} role="status">
                            {checking
                                ? 'Checking…'
                                : status?.reachable
                                    ? `Connected. ${models.length} model${models.length === 1 ? '' : 's'} installed.`
                                    : status?.error ?? ''}
                        </p>

                        {models.length > 0 && (
                            <Select
                                label="Model"
                                value={model}
                                options={models.map(name => ({ value: name, label: name }))}
                                onChange={setModel}
                            />
                        )}

                        {!checking && status?.reachable && models.length === 0 && (
                            <p className="settings-dialog-hint">
                                No models are installed. Run <code className="settings-dialog-code">ollama pull {suggestedModel}</code> in a terminal, then press Check.
                            </p>
                        )}
                        {!checking && status && !status.reachable && (
                            <p className="settings-dialog-hint">
                                Install Ollama from ollama.com and start it, then pull a model, for example
                                {' '}<code className="settings-dialog-code">ollama pull {suggestedModel}</code>.
                            </p>
                        )}
                    </div>
                )}

                {about && (
                    <div className="settings-dialog-group settings-dialog-about">
                        <span className="settings-dialog-label">About</span>
                        <p className="settings-dialog-version">Smart CV {about.version}</p>
                        <p className="settings-dialog-hint">
                            Your profiles, templates and exports are stored in
                            {' '}<code className="settings-dialog-code settings-dialog-folder">{about.dataFolder}</code>.
                            Copy this folder to back them up.
                        </p>
                    </div>
                )}
            </div>
        </Modal>
    );
};

export default SettingsDialog;
